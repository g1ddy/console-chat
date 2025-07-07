using System.Text.Json.Serialization;

namespace RaindropServer.Tags;

public record TagRenameRequest
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Replace { get; init; }

    public List<string> Tags { get; init; } = new();
}
