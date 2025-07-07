using System.Text.Json.Serialization;
using RaindropServer.Common;

namespace RaindropServer.Collections;

public class Collection
{
    [JsonPropertyName("_id")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public int Id { get; set; }

    public string? Title { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public IdRef? Parent { get; set; }

    public string? Color { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? Cover { get; set; }

    public bool? Public { get; set; }
}
