using System.Text.Json.Serialization;

namespace AzureServices.Delta;

public class SqlWarehouseQuery
{
    public SqlWarehouseQuery(string warehouseId, string schema, string statement, string catalog, string disposition)
    {
        WarehouseId = warehouseId;
        Schema = schema;
        Statement = statement;
        Catalog = catalog;
        Disposition = disposition;
    }

    [JsonPropertyName("warehouse_id")]
    public string WarehouseId { get; set; }

    [JsonPropertyName("catalog")]
    public string Catalog { get; set; }

    [JsonPropertyName("schema")]
    public string Schema { get; set; }

    [JsonPropertyName("statement")]
    public string Statement { get; set; }

    [JsonPropertyName("disposition")]
    public string Disposition { get; set; }
}