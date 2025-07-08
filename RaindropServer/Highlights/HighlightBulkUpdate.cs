using System.Text.Json.Serialization;

namespace RaindropServer.Highlights;

public record HighlightBulkUpdate
{
    [JsonPropertyName("_id")]
    public string? Id { get; init; }

    public string? Text { get; init; }

    public string? Note { get; init; }

    public string? Color { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Created { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? LastUpdate { get; init; }
}
