using System.ComponentModel;
using System.Text.Json.Serialization;

namespace RaindropServer.Filters;

/// <summary>
/// Represents a saved filter (smart view) in Raindrop.io.
/// </summary>
[Description("Saved filter definition")]
public record Filter
{
    [JsonPropertyName("_id")]
    [Description("Filter identifier")]
    public int Id { get; init; }

    [Description("Filter title")]
    public string? Title { get; init; }

    [Description("Query expression defining the filter")]
    public string? Query { get; init; }
}
