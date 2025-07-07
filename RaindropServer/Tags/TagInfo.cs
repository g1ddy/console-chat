using System.Text.Json.Serialization;

namespace RaindropServer.Tags;

public record TagInfo
{
    [JsonPropertyName("_id")]
    public string Id { get; init; } = string.Empty;

    public int Count { get; init; }
}
