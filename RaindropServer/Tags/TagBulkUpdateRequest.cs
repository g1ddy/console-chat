using System.Text.Json.Serialization;

namespace RaindropServer.Tags;

/// <summary>
/// Request model for bulk tag updates. When <see cref="Replace"/> is null, the
/// request represents a deletion of the provided tags.
/// </summary>
public class TagBulkUpdateRequest
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Replace { get; set; }

    public List<string> Tags { get; set; } = new();
}
