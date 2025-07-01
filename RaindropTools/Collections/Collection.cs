using System.Text.Json.Serialization;
using RaindropTools.Common;

namespace RaindropTools.Collections;

public class Collection
{
    [JsonPropertyName("_id")]
    public int Id { get; set; }

    public string? Title { get; set; }

    public ParentRef? Parent { get; set; }

    public string? Color { get; set; }

    public List<string>? Cover { get; set; }

    [JsonPropertyName("public")]
    public bool? Public { get; set; }
}
