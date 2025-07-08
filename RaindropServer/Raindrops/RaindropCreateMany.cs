using System.Text.Json.Serialization;

namespace RaindropServer.Raindrops;

public record RaindropCreateMany
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? CollectionId { get; init; }

    public List<Raindrop> Items { get; init; } = new();
}
