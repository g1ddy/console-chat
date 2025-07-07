using System.Text.Json.Serialization;

namespace RaindropServer.Raindrops;

public class RaindropsCreateMany
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? CollectionId { get; set; }

    public List<Raindrop> Items { get; set; } = new();
}
