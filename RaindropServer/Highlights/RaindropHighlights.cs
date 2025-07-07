using System.Text.Json.Serialization;

namespace RaindropServer.Highlights;

public record RaindropHighlights
{
    [JsonPropertyName("_id")]
    public long? Id { get; init; }

    public List<Highlight> Highlights { get; init; } = new();
}
