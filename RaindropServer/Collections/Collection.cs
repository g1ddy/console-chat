using System.Text.Json.Serialization;
using RaindropServer.Common;

namespace RaindropServer.Collections;

public record Collection
{
    [JsonPropertyName("_id")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public int Id { get; init; }

    public string? Title { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public IdRef? Parent { get; init; }

    public string? Color { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? Cover { get; init; }

    public bool? Public { get; init; }
}
