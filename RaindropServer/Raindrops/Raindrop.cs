using System.Text.Json.Serialization;
using RaindropTools.Common;

namespace RaindropTools.Raindrops;

public class Raindrop
{
    [JsonPropertyName("_id")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public long Id { get; set; }

    public string? Title { get; set; }

    public string? Link { get; set; }

    public string? Excerpt { get; set; }

    public List<string>? Tags { get; set; }

    public bool? Important { get; set; }

    public IdRef? Collection { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? CollectionId { get; set; }
}
