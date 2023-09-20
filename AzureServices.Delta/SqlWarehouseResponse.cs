using System.Text.Json.Serialization;

namespace AzureServices.Delta;

public class SqlWarehouseResponse
{
    public SqlWarehouseResponse(string statementId, Status status, Manifest manifest, Result result)
    {
        StatementId = statementId;
        Status = status;
        Manifest = manifest;
        Result = result;
    }

    [JsonPropertyName("statement_id")]
    public string StatementId { get; set; }

    [JsonPropertyName("status")]
    public Status Status { get; set; }

    [JsonPropertyName("manifest")]
    public Manifest? Manifest { get; set; }

    [JsonPropertyName("result")]
    public Result? Result { get; set; }
}