using System.Text.Json.Serialization;
using RaindropTools.Common;

namespace RaindropTools.Raindrops;

public class Raindrop
{
    [JsonPropertyName("_id")]
    public long Id { get; set; }

    public string? Title { get; set; }

    public string? Link { get; set; }

    public string? Excerpt { get; set; }

    public List<string>? Tags { get; set; }

    public bool? Important { get; set; }

    public ParentRef? Collection { get; set; }
}
