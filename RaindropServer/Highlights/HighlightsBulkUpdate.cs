using System.Text.Json.Serialization;

namespace RaindropServer.Highlights;

public class HighlightsBulkUpdate
{
    [JsonPropertyName("_id")]
    public string? Id { get; set; }

    public string? Text { get; set; }

    public string? Note { get; set; }

    public string? Color { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Created { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? LastUpdate { get; set; }
}
