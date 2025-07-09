using System.Text.Json.Serialization;

namespace RaindropServer.Raindrops;

/// <summary>
/// Request payload for creating multiple bookmarks.
/// </summary>
public record RaindropCreateManyRequest
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? CollectionId { get; init; }

    public List<Raindrop> Items { get; init; } = new();
}
