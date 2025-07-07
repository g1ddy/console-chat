using System.Text.Json.Serialization;

namespace RaindropServer.Highlights;

public class RaindropHighlights
{
    [JsonPropertyName("_id")]
    public long? Id { get; set; }

    public List<Highlight> Highlights { get; set; } = new();
}
