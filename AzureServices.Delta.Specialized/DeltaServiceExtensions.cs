using Azure.Storage.Blobs;

namespace AzureServices.Delta.Specialized;

public static class DeltaServiceExtensions
{
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
}