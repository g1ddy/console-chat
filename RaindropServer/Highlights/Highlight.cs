using System.Text.Json.Serialization;

namespace RaindropTools.Highlights;

public class Highlight
{
    [JsonPropertyName("_id")]
    public string? Id { get; set; }

    public string? Text { get; set; }

    public string? Title { get; set; }

    public string? Color { get; set; }

    public string? Note { get; set; }

    public string? Created { get; set; }

    public List<string>? Tags { get; set; }

    public string? Link { get; set; }

    public string? LastUpdate { get; set; }

    [JsonPropertyName("raindropRef")]
    public long? RaindropRef { get; set; }
}
