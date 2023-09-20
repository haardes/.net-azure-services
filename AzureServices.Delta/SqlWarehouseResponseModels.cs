using System.Text.Json.Serialization;

namespace AzureServices.Delta;

public class Chunk
{
    public Chunk(int chunkIndex, int rowOffset, long rowCount, long byteCount)
    {
        ChunkIndex = chunkIndex;
        RowOffset = rowOffset;
        RowCount = rowCount;
        ByteCount = byteCount;
    }

    [JsonPropertyName("chunk_index")]
    public int ChunkIndex { get; set; }

    [JsonPropertyName("row_offset")]
    public int RowOffset { get; set; }

    [JsonPropertyName("row_count")]
    public long RowCount { get; set; }

    [JsonPropertyName("byte_count")]
    public long ByteCount { get; set; }

}

public class Column
{
    public Column(string name, string typeText, string typeName, int position)
    {
        Name = name;
        TypeText = typeText;
        TypeName = typeName;
        Position = position;
    }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("type_text")]
    public string TypeText { get; set; }

    [JsonPropertyName("type_name")]
    public string TypeName { get; set; }

    [JsonPropertyName("position")]
    public int Position { get; set; }

    public int CompareTo(Column column)
    {
        return Position - column.Position;
    }
}

public class Link
{
    public Link(int chunkIndex, int rowOffset, int rowCount, long byteCount, int nextChunkIndex, string nextChunkInternalLink, string externalLink, DateTime expiration)
    {
        ChunkIndex = chunkIndex;
        RowOffset = rowOffset;
        RowCount = rowCount;
        ByteCount = byteCount;
        NextChunkIndex = nextChunkIndex;
        NextChunkInternalLink = nextChunkInternalLink;
        ExternalLink = externalLink;
        Expiration = expiration;
    }

    [JsonPropertyName("chunk_index")]
    public int ChunkIndex { get; set; }

    [JsonPropertyName("row_offset")]
    public int RowOffset { get; set; }

    [JsonPropertyName("row_count")]
    public int RowCount { get; set; }

    [JsonPropertyName("byte_count")]
    public long ByteCount { get; set; }

    [JsonPropertyName("next_chunk_index")]
    public int NextChunkIndex { get; set; }

    [JsonPropertyName("next_chunk_internal_link")]
    public string NextChunkInternalLink { get; set; }

    [JsonPropertyName("external_link")]
    public string ExternalLink { get; set; }

    [JsonPropertyName("expiration")]
    public DateTime Expiration { get; set; }

}

public class Manifest
{
    public Manifest(string format, Schema schema, int totalChunkCount, List<Chunk> chunks, int totalRowCount, long totalByteCount)
    {
        Format = format;
        Schema = schema;
        TotalChunkCount = totalChunkCount;
        Chunks = chunks;
        TotalRowCount = totalRowCount;
        TotalByteCount = totalByteCount;
    }

    [JsonPropertyName("format")]
    public string Format { get; set; }

    [JsonPropertyName("schema")]
    public Schema Schema { get; set; }

    [JsonPropertyName("total_chunk_count")]
    public int TotalChunkCount { get; set; }

    [JsonPropertyName("chunks")]
    public List<Chunk> Chunks { get; set; }

    [JsonPropertyName("total_row_count")]
    public int TotalRowCount { get; set; }

    [JsonPropertyName("total_byte_count")]
    public long TotalByteCount { get; set; }

}

public class Result
{
    public Result(List<Link> externalLinks)
    {
        ExternalLinks = externalLinks;
    }

    [JsonPropertyName("external_links")]
    public List<Link> ExternalLinks { get; set; }

}

public class Schema
{
    public Schema(int columnCount, List<Column> columns)
    {
        ColumnCount = columnCount;
        Columns = columns;
    }

    [JsonPropertyName("column_count")]
    public int ColumnCount { get; set; }

    [JsonPropertyName("columns")]
    public List<Column> Columns { get; set; }

}

public class Status
{
    public Status(string state, Error error)
    {
        State = state;
        Error = error;
    }

    [JsonPropertyName("state")]
    public string State { get; set; }

    [JsonPropertyName("error")]
    public Error Error { get; set; }
}

public class Error
{
    public Error(string errorCode, string message)
    {
        ErrorCode = errorCode;
        Message = message;
    }

    [JsonPropertyName("error_code")]
    public string ErrorCode { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; }
}