using System.Text.Json.Serialization;

namespace AzureServices.Delta;

public class QueryParameters
{
    public QueryParameters(string name, string value)
    {
        Name = name;
        Value = value;
        Type = "string";
    }

    public QueryParameters(string name, string value, string type)
    {
        Name = name;
        Value = value;
        Type = type;
    }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("value")]
    public string Value { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }
}