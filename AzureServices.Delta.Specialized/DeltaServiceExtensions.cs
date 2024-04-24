using System.Net;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;

namespace AzureServices.Delta.Specialized;

public static class DeltaServiceExtensions
{
    // TODO: Handle empty results (no rows found)

    public static async Task WriteDeltaTableToBlob(this IDeltaService deltaService, BlobClient blob, string schema, string statement, string catalog = "hive_metastore", string disposition = "EXTERNAL_LINKS", string filename = "")
    {
        (bool IsInitialized, string Message) = deltaService.IsInitialized();
        if (!IsInitialized)
        {
            throw new Exception(Message);
        }

        SqlWarehouseQuery query = new(deltaService.WarehouseId!, schema, statement, catalog, disposition);
        SqlWarehouseResponse metadata = deltaService.FetchMetadataAndAwaitSuccess(query);
        bool headersWritten = false;

        StreamWriter sw = new(blob.OpenWrite(true));

        Result? currentResult = metadata.Result ?? throw new NullReferenceException("Warehouse returned no Result-object. Check query and connection details.");
        while (currentResult != null)
        {
            string csv = deltaService.FetchCsvFromResult(currentResult, metadata, ref headersWritten);
            await sw.WriteAsync(csv);
            sw.Flush();
            currentResult = deltaService.FetchNextResult(currentResult);

            GC.Collect();
        }

        await sw.DisposeAsync();
    }

    public static async Task WriteDeltaTableToResponse(this IDeltaService deltaService, HttpResponse response, string schema, string statement, string catalog = "hive_metastore", string disposition = "EXTERNAL_LINKS", string filename = "")
    {
        (bool IsInitialized, string Message) = deltaService.IsInitialized();
        if (!IsInitialized)
        {
            throw new Exception(Message);
        }

        SqlWarehouseQuery query = new(deltaService.WarehouseId!, schema, statement, catalog, disposition);
        SqlWarehouseResponse metadata = deltaService.FetchMetadataAndAwaitSuccess(query);
        bool headersWritten = false;

        response.StatusCode = (int)HttpStatusCode.OK;
        response.ContentType = "application/octet-stream";

        if (string.IsNullOrEmpty(filename)) filename = schema;
        response.Headers.Add("Content-Disposition", $"attachment; filename=\"{filename}\"");

        StreamWriter sw = new(response.Body);

        Result? currentResult = metadata.Result ?? throw new NullReferenceException("Warehouse returned no Result-object. Check query and connection details.");
        while (currentResult != null)
        {
            string csv = deltaService.FetchCsvFromResult(currentResult, metadata, ref headersWritten);
            await sw.WriteAsync(csv);
            currentResult = deltaService.FetchNextResult(currentResult);

            GC.Collect();
        }

        await sw.DisposeAsync();
    }
}