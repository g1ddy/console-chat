using System.Text.Json.Serialization;

namespace RaindropServer.Tags;

public class TagRenameRequest
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Replace { get; set; }

    public List<string> Tags { get; init; } = new();
}
