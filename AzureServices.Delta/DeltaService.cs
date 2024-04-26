using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;
using System.Net;
using System.Text.Json;
using AzureServices.Core;
using Microsoft.Extensions.Configuration;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

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

    public string GetDeltaTableContent(string schema, string statement, string catalog = "hive_metastore", string disposition = "EXTERNAL_LINKS", string format = "csv")
    {
        (bool IsInitialized, string Message) status = IsInitialized();
        if (!status.IsInitialized)
        {
            throw new Exception(status.Message);
        }

        SqlWarehouseQuery query = new(_warehouseId!, schema, statement, catalog, disposition);
        SqlWarehouseResponse metadata = FetchMetadataAndAwaitSuccess(query);

        if (format.ToLower() == "csv")
        {
            string csv = GetCsvFromMetadata(metadata);

            return csv;
        }

        if (format.ToLower() == "json")
        {
            string json = GetJsonFromMetadata(metadata);

            return json;
        }

        throw new ArgumentException("Format not recognized.", nameof(format));
    }

    public string GetDeltaTableContent(string schema, string statement, IEnumerable<QueryParameters> parameters, string catalog = "hive_metastore", string disposition = "EXTERNAL_LINKS", string format = "csv")
    {
        (bool IsInitialized, string Message) status = IsInitialized();
        if (!status.IsInitialized)
        {
            throw new Exception(status.Message);
        }

        SqlWarehouseQuery query = new(_warehouseId!, schema, statement, catalog, disposition, parameters);
        SqlWarehouseResponse metadata = FetchMetadataAndAwaitSuccess(query);

        if (format.ToLower() == "csv")
        {
            string csv = GetCsvFromMetadata(metadata);

            return csv;
        }

        if (format.ToLower() == "json")
        {
            string json = GetJsonFromMetadata(metadata);

            return json;
        }

        throw new ArgumentException("Format not recognized.", nameof(format));
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

    private string GetJsonFromMetadata(SqlWarehouseResponse metadata)
    {
        JsonObject root = new();
        JsonArray data = new();

        List<(string TypeText, string TypeName)> types = ExtractTypes(metadata);
        List<string> headers = ExtractHeaders(metadata);

        Result? currentResult = metadata.Result ?? throw new NullReferenceException("Warehouse returned no Result-object. Check query and connection details.");

        while (currentResult != null)
        {
            string? externalLink = currentResult.ExternalLinks.FirstOrDefault()?.ExternalLink;
            if (string.IsNullOrEmpty(externalLink))
            {
                throw new ArgumentException($"Warehouse-response contains no external link for statement {metadata.StatementId}. Check query.");
            }

            HttpRequestMessage dataRequest = new(HttpMethod.Get, externalLink);
            HttpResponseMessage dataResponse = _deltaClient.Send(dataRequest);
            string result = dataResponse.Content.ReadAsStringAsync().Result;

            string[] csvRows = result.Replace("[[", "").Replace("]]", "").Split("],[");

            foreach (string row in csvRows)
            {
                string[] fields = SplitAtCommasOutsideQuotationMarksAndObjectsAndArraysAndGeneric(row);

                JsonObject entry = new();

                for (int i = 0; i < fields.Length; i++)
                {
                    (string, string) type = types[i];
                    string column = headers[i];
                    string field = fields[i].Trim('"');

                    entry.Add(column, ConvertFieldToJson(type, field));
                }

                data.Add(entry);
            }

            currentResult = FetchNextResult(currentResult);
        }

        root.Add("data", data);
        root.Add("antall", data.Count);

        return root.ToJsonString(new()
        {
            WriteIndented = true
        });
    }

    private static string[] SplitAtCommasOutsideQuotationMarksAndObjectsAndArraysAndGeneric(string row)
    {
        List<string> result = new();

        int quotationMarksSeen = 0;
        int startIndex = 0;
        bool hasSeenStartOfArray = false;
        bool hasSeenStartOfObject = false;
        bool hasSeenStartOfGeneric = false;

        for (int i = 0; i < row.Length; i++)
        {
            char c = row[i];

            if (c.Equals('[') && !hasSeenStartOfObject && !hasSeenStartOfGeneric)
            {
                hasSeenStartOfArray = true;
            }

            if (c.Equals(']') && hasSeenStartOfArray && !hasSeenStartOfObject && !hasSeenStartOfGeneric)
            {
                hasSeenStartOfArray = false;
            }

            if (c.Equals('{') && !hasSeenStartOfArray && !hasSeenStartOfGeneric)
            {
                hasSeenStartOfObject = true;
            }

            if (c.Equals('}') && hasSeenStartOfObject && !hasSeenStartOfArray && !hasSeenStartOfGeneric)
            {
                hasSeenStartOfObject = false;
            }

            if (c.Equals('<') && !hasSeenStartOfArray && !hasSeenStartOfObject)
            {
                hasSeenStartOfGeneric = true;
            }

            if (c.Equals('>') && hasSeenStartOfGeneric && !hasSeenStartOfArray && !hasSeenStartOfObject)
            {
                hasSeenStartOfGeneric = false;
            }

            if (c.Equals('"'))
            {
                quotationMarksSeen++;
            }

            if (c.Equals(','))
            {
                if (!hasSeenStartOfArray && !hasSeenStartOfObject && !hasSeenStartOfGeneric && quotationMarksSeen % 2 == 0)
                {
                    result.Add(row[startIndex..i]);
                    startIndex = i + 1;
                }
            }

            if (i == row.Length - 1)
            {
                result.Add(row[startIndex..row.Length]);
            }
        }

        return result.ToArray();
    }

    private JsonNode? ConvertFieldToJson((string TypeText, string TypeName) type, string field)
    {
        try
        {
            switch (type.TypeName)
            {
                case "STRING":
                    return field;
                case "BYTE":
                    return byte.Parse(field);
                case "SHORT":
                    return short.Parse(field);
                case "INT":
                    return int.Parse(field);
                case "LONG":
                    return long.Parse(field);
                case "FLOAT":
                    return float.Parse(field);
                case "DOUBLE":
                    return double.Parse(field);
                case "DECIMAL":
                    return decimal.Parse(field);
                case "BOOLEAN":
                    return bool.Parse(field);
                case "TIMESTAMP":
                    return field;
                case "ARRAY":
                    return ParseArrayToJson(type, field);
                case "MAP":
                    return ParseMapToJson(type, field);
                case "STRUCT":
                    return ParseStructToJson(type, field);
                default:
                    return field;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return null;
        }
    }

    private JsonNode? ParseArrayToJson((string TypeText, string TypeName) type, string field)
    {
        JsonArray jsonArray = new();

        int firstArrowIndex = type.TypeText.IndexOf('<');
        int lastArrowIndex = type.TypeText.LastIndexOf('>');

        //Inner type_text = string between first and last arrows (<>)
        string innerTypeText = type.TypeText[(firstArrowIndex + 1)..lastArrowIndex];

        //Inner type_name = string before first arrow (<) if an arrow exists, if not: same as innerTypeText
        int firstInnerArrow = innerTypeText.IndexOf('<');
        string innerTypeName = firstInnerArrow > -1 ? innerTypeText[..firstInnerArrow] : innerTypeText;

        // Values = field without [] and split at commas (each entry needs to be trimmed as before (remove " around each entry))
        List<string> arrayValues = SplitAtCommasOutsideQuotationMarksAndObjectsAndArraysAndGeneric(field.Trim('[', ']'))
                                    .Select(val => val.Trim('"', '\\'))
                                    .ToList();

        arrayValues.ForEach(val => jsonArray.Add(ConvertFieldToJson((innerTypeText, innerTypeName), val)));

        return jsonArray;
    }

    private JsonNode? ParseMapToJson((string TypeText, string TypeName) type, string field)
    {
        JsonObject jsonMap = new();

        int firstArrowIndex = type.TypeText.IndexOf('<');
        int lastArrowIndex = type.TypeText.LastIndexOf('>');

        //Inner type = string between first and last arrows (<>)
        // * Note that innerTypeText will be keyType + valueType ie. "STRING, INT" or "INT, ARRAY<STRING>"
        string innerTypeText = type.TypeText[(firstArrowIndex + 1)..lastArrowIndex];

        // * We don't care about keyType, JSON only allows strings as properties for objects (which a map must be sent as)
        // * Because valueType may be of a nested structure, ie. "STRING, MAP<STRING, INT>", we need to find everything AFTER the first ","
        int indexOfComma = innerTypeText.IndexOf(',');
        string valueType = innerTypeText[(indexOfComma + 1)..].Trim();

        //Inner type_name = string before first arrow (<) if an arrow exists, if not: same as innerTypeText
        int firstInnerArrow = valueType.IndexOf('<');
        string innerTypeName = firstInnerArrow > -1 ? valueType[..firstInnerArrow] : valueType;

        JsonNode obj = JsonNode.Parse(Regex.Unescape(field))!;

        foreach (var key in obj.AsObject())
        {
            jsonMap.Add(key.Key, ConvertFieldToJson((valueType, innerTypeName), key.Value!.ToJsonString()));
        }

        return jsonMap;
    }

    private JsonNode? ParseStructToJson((string TypeText, string TypeName) type, string field)
    {
        JsonObject jsonStruct = new();

        int firstArrowIndex = type.TypeText.IndexOf('<');
        int lastArrowIndex = type.TypeText.LastIndexOf('>');

        //Inner type = string between first and last arrows (<>)
        // * Note that innerTypeText will be fieldname + valueType ie. "Field1: STRING, Field2: INT" or "Field1: INT, Field2: ARRAY<STRING>"
        string innerTypeText = type.TypeText[(firstArrowIndex + 1)..lastArrowIndex];
        string[] innerTypeTexts = SplitAtCommasOutsideQuotationMarksAndObjectsAndArraysAndGeneric(innerTypeText);

        JsonNode obj = JsonNode.Parse(Regex.Unescape(field))!;

        foreach (string inner in innerTypeTexts)
        {
            int firstSeparator = inner.IndexOf(":");
            string column = inner[..firstSeparator].Trim();
            string actualType = inner[(firstSeparator + 1)..].Trim();

            //Inner type_name = string before first arrow (<) if an arrow exists, if not: same as valueType
            int firstInnerArrow = actualType.IndexOf('<');
            string innerTypeName = firstInnerArrow > -1 ? actualType[..firstInnerArrow] : actualType;

            jsonStruct.Add(column, ConvertFieldToJson((actualType, innerTypeName), obj[column]!.ToJsonString().Trim('"')));
        }

        return jsonStruct;
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

    private static List<(string TypeText, string TypeName)> ExtractTypes(SqlWarehouseResponse response)
    {
        List<(string TypeText, string TypeName)> types = new();

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
            types.Add((column.TypeText, column.TypeName));
        }

        return types;
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