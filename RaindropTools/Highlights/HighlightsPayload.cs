namespace RaindropTools.Highlights;

public class HighlightsPayload
{
    public List<HighlightInput> Highlights { get; set; } = new();
}

public class HighlightInput
{
    [System.Text.Json.Serialization.JsonPropertyName("_id")]
    public string? Id { get; set; }
    public string? Text { get; set; }
    public string? Note { get; set; }
    public string? Color { get; set; }
}
