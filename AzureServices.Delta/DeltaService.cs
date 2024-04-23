using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;
using System.Net;
using System.Text.Json;
using AzureServices.Core;
using Microsoft.Extensions.Configuration;

namespace AzureServices.Delta;

public class DeltaService : IDeltaService
{
    private readonly string? _workspaceId;
    private readonly string? _warehouseId;
    private readonly string? _warehouseToken;
    private readonly string? _workspaceUri;
    private readonly HttpClient _deltaClient = new();

    public string? WarehouseId
    {
        get
        {
            return _warehouseId;
        }
    }

    public DeltaService()
    {
    }

    public DeltaService(Action<DeltaService> configureService)
    {
        configureService.Invoke(this);
    }

    public DeltaService(string? workspaceId, string? warehouseId, string? warehouseToken)
    {
        _workspaceId = workspaceId;
        _warehouseId = warehouseId;
        _warehouseToken = warehouseToken;
        _workspaceUri = $"https://adb-{_workspaceId}.azuredatabricks.net";
    }

    public DeltaService(IKeyVaultService keyVaultService, KeyOptions? options = null)
    {
        if (options == null)
        {
            _workspaceId = keyVaultService.GetSecret("DatabricksWorkspaceId");
            _warehouseId = keyVaultService.GetSecret("DatabricksWarehouseId");
            _warehouseToken = keyVaultService.GetSecret("DatabricksApiToken");
            _workspaceUri = $"https://adb-{_workspaceId}.azuredatabricks.net";
        }
        else
        {
            _workspaceId = keyVaultService.GetSecret(options.DatabricksWorkspaceId());
            _warehouseId = keyVaultService.GetSecret(options.DatabricksWarehouseId());
            _warehouseToken = keyVaultService.GetSecret(options.DatabricksApiToken());
            _workspaceUri = $"https://adb-{_workspaceId}.azuredatabricks.net";
        }
    }

#pragma warning disable CA1822 // Mark members as static
    public DeltaService AddConfiguration(IConfiguration configuration, KeyOptions? options = null)
#pragma warning restore CA1822 // Mark members as static
    {
        if (options == null)
        {
            return new DeltaService(configuration["DatabricksWorkspaceId"], configuration["DatabricksWarehouseId"], configuration["DatabricksApiToken"]);
        }
        else
        {
            return new DeltaService(configuration[options.DatabricksWorkspaceId()], configuration[options.DatabricksWarehouseId()], configuration[options.DatabricksApiToken()]);
        }
    }

    public DeltaService AddAzureKeyVault(string keyVaultUri, KeyOptions? options = null)
    {
        SecretClient secretClient = new(new(keyVaultUri), new DefaultAzureCredential());
        return AddAzureKeyVault(secretClient, options);
    }

#pragma warning disable CA1822 // Mark members as static
    public DeltaService AddAzureKeyVault(IKeyVaultService keyVaultService, KeyOptions? options = null)
#pragma warning restore CA1822 // Mark members as static
    {
        if (options == null)
        {
            return new DeltaService(
                keyVaultService.GetSecret("DatabricksWorkspaceId"),
                keyVaultService.GetSecret("DatabricksWarehouseId"),
                keyVaultService.GetSecret("DatabricksApiToken")
            );
        }
        else
        {
            return new DeltaService(
                keyVaultService.GetSecret(options.DatabricksWorkspaceId()),
                keyVaultService.GetSecret(options.DatabricksWarehouseId()),
                keyVaultService.GetSecret(options.DatabricksApiToken())
            );
        }
    }

    public DeltaService AddAzureKeyVault(SecretClient secretClient, KeyOptions? options = null)
    {
        if (options == null)
        {
            return new DeltaService(
                secretClient.GetSecret("DatabricksWorkspaceId").Value.Value,
                secretClient.GetSecret("DatabricksWarehouseId").Value.Value,
                secretClient.GetSecret("DatabricksApiToken").Value.Value
            );
        }
        else
        {
            return new DeltaService(
                secretClient.GetSecret(options.DatabricksWorkspaceId()).Value.Value,
                secretClient.GetSecret(options.DatabricksWarehouseId()).Value.Value,
                secretClient.GetSecret(options.DatabricksApiToken()).Value.Value
            );
        }
    }

    public string GetDeltaTableContent(string schema, string statement, string catalog = "hive_metastore", string disposition = "EXTERNAL_LINKS")
    {
        (bool IsInitialized, string Message) status = IsInitialized();
        if (!status.IsInitialized)
        {
            throw new Exception(status.Message);
        }

        SqlWarehouseQuery query = new(_warehouseId!, schema, statement, catalog, disposition);
        SqlWarehouseResponse metadata = FetchMetadataAndAwaitSuccess(query);

        string csv = GetCsvFromMetadata(metadata);

        return csv;
    }

    // TODO: Handle empty results (no rows found)

    public async Task WriteDeltaTableToResponse(HttpResponse response, string schema, string statement, string catalog = "hive_metastore", string disposition = "EXTERNAL_LINKS", string filename = "")
    {
        (bool IsInitialized, string Message) status = IsInitialized();
        if (!status.IsInitialized)
        {
            throw new Exception(status.Message);
        }

        SqlWarehouseQuery query = new(_warehouseId!, schema, statement, catalog, disposition);
        SqlWarehouseResponse metadata = FetchMetadataAndAwaitSuccess(query);
        bool headersWritten = false;

        response.StatusCode = (int)HttpStatusCode.OK;
        response.ContentType = "application/octet-stream";

        if (string.IsNullOrEmpty(filename)) filename = schema;
        response.Headers.Add("Content-Disposition", $"attachment; filename=\"{filename}-data.csv\"");

        StreamWriter sw = new(response.Body);

        Result? currentResult = metadata.Result ?? throw new NullReferenceException("Warehouse returned no Result-object. Check query and connection details.");
        while (currentResult != null)
        {
            string csv = FetchCsvFromResult(currentResult, metadata, ref headersWritten);
            await sw.WriteAsync(csv);
            currentResult = FetchNextResult(currentResult);

            GC.Collect();
        }

        await sw.DisposeAsync();
    }
    public SqlWarehouseResponse FetchMetadataAndAwaitSuccess(SqlWarehouseQuery query)
    {
        HttpRequestMessage request = new(HttpMethod.Post, _workspaceUri + "/api/2.0/sql/statements")
        {
            Content = new StringContent(JsonSerializer.Serialize(query)),
            Headers = {
                Authorization = new AuthenticationHeaderValue("Bearer", _warehouseToken!)
            }
        };

        HttpResponseMessage response = _deltaClient.Send(request);
        request.Dispose();
        string json = response.Content.ReadAsStringAsync().Result;

        if (string.IsNullOrEmpty(json))
        {
            throw new ArgumentNullException("json", "Parsing of content as string returned null or empty string.");
        }

        var warehouseResponse = JsonSerializer.Deserialize<SqlWarehouseResponse>(json) ?? throw new Exception("Deserialization of json-response returned null.");

        var status = warehouseResponse.Status;

        if (status.State == "PENDING")
        {
            Thread.Sleep(5000);
            return FetchMetadataAndAwaitSuccess(query);
        }

        if (status.State == "FAILED")
        {
            throw new Exception(status.Error.Message);
        }

        return warehouseResponse;
    }

    private string GetCsvFromMetadata(SqlWarehouseResponse metadata)
    {
        string csv = "";
        bool hasHeaders = false;

        Result? currentResult = metadata.Result ?? throw new NullReferenceException("Warehouse returned no Result-object. Check query and connection details.");

        while (currentResult != null)
        {
            csv += FetchCsvFromResult(currentResult, metadata, ref hasHeaders);
            currentResult = FetchNextResult(currentResult);
        }

        return csv;
    }

    public Result? FetchNextResult(Result currentResult)
    {
        string? nextChunkInternalLink = currentResult.ExternalLinks.FirstOrDefault()?.NextChunkInternalLink;
        if (string.IsNullOrEmpty(nextChunkInternalLink))
        {
            return null;
        }

        Console.WriteLine($"{_workspaceUri + nextChunkInternalLink}");
        HttpRequestMessage resultRequest = new(HttpMethod.Get, _workspaceUri + nextChunkInternalLink)
        {
            Headers = {
                    Authorization = new AuthenticationHeaderValue("Bearer", _warehouseToken!)
                }
        };
        HttpResponseMessage resultResponse = _deltaClient.Send(resultRequest);
        string json = resultResponse.Content.ReadAsStringAsync().Result;

        return JsonSerializer.Deserialize<Result>(json);
    }

    public string FetchCsvFromResult(Result currentResult, SqlWarehouseResponse metadata, ref bool hasHeaders)
    {
        string? externalLink = currentResult.ExternalLinks.FirstOrDefault()?.ExternalLink;
        if (string.IsNullOrEmpty(externalLink))
        {
            throw new ArgumentException($"Warehouse-response contains no external link for statement {metadata.StatementId}. Check query.");
        }

        Console.WriteLine($"{externalLink}");
        HttpRequestMessage dataRequest = new(HttpMethod.Get, externalLink);
        HttpResponseMessage dataResponse = _deltaClient.Send(dataRequest);
        string result = dataResponse.Content.ReadAsStringAsync().Result;

        if (hasHeaders)
        {
            return FormatResponseAsCsv(result);
        }
        else
        {
            hasHeaders = true;
            return FormatResponseAsCsv(result, ExtractHeaders(metadata));
        }
    }

    /// <summary>
    /// The <see cref="ExtractHeaders"/> operation extracts column headers from a <see cref="SqlWarehouseResponse"/>.
    /// </summary>
    /// <param name="response"></param>
    /// <remarks>
    /// <para>A <see cref="ArgumentException"/> will be thrown if the response does not contain any columns in the schema.</para>
    /// </remarks>
    private static List<string> ExtractHeaders(SqlWarehouseResponse response)
    {
        List<string> headers = new();

        List<Column>? columns = response.Manifest?.Schema?.Columns;

        if (columns == null || columns.Count == 0)
        {
            throw new ArgumentException("Columns list is empty, no columns found in schema.");
        }

        // Ensure elements are in order of their position in the table
        columns.Sort((first, second) =>
        {
            return first.CompareTo(second);
        });

        foreach (Column column in columns)
        {
            headers.Add(column.Name);
        }

        return headers;
    }

    /// <summary>
    /// The <see cref="FormatResponseAsCsv"/> operation replaces string content to produce a CSV-formatted string.
    /// </summary>
    /// <param name="headers"></param>
    /// <param name="text"></param>
    private static string FormatResponseAsCsv(string text, List<string>? headers = null)
    {
        if (headers == null)
        {
            text = text.Replace("[[", "");
        }
        else
        {
            text = text.Replace("[[", $"{string.Join(",", headers)}\n");
        }

        text = text.Replace("]]", "\n");
        text = text.Replace("],[", "\n");

        return text;
    }

    public (bool IsInitialized, string Message) IsInitialized()
    {
        string message = "";

        if (string.IsNullOrEmpty(_warehouseId))
        {
            message += "Missing warehouseId. ";
        }

        if (string.IsNullOrEmpty(_workspaceId))
        {
            message += "Missing workspaceId. ";
        }

        if (string.IsNullOrEmpty(_warehouseToken))
        {
            message += "Missing warehouseToken. ";
        }

        if (string.IsNullOrEmpty(message))
        {
            return (true, message);
        }

        return (false, message);
    }
}