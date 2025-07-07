using System.Text.Json.Serialization;

namespace RaindropTools.Tags;

public class TagInfo
{
    [JsonPropertyName("_id")]
    public string Id { get; set; } = string.Empty;

    public int Count { get; set; }
}
