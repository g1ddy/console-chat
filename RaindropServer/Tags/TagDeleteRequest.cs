using System.ComponentModel;

namespace RaindropServer.Tags;

/// <summary>
/// Payload for deleting tags.
/// </summary>
[Description("Tag delete request")]
public record TagDeleteRequest
{
    [Description("Tags to remove")]
    public IEnumerable<string> Tags { get; init; } = Array.Empty<string>();
}
