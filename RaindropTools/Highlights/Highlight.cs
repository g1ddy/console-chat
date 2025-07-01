using System.Text.Json.Serialization;

namespace RaindropTools.Highlights;

public class Highlight
{
    [JsonPropertyName("_id")]
    public string Id { get; set; } = string.Empty;

    public string Text { get; set; } = string.Empty;

    public string? Title { get; set; }

    public string Color { get; set; } = "yellow";

    public string? Note { get; set; }

    public string? Created { get; set; }

    public List<string>? Tags { get; set; }

    public string? Link { get; set; }

    public string? LastUpdate { get; set; }
}
