using System.Text.Json.Serialization;

namespace RaindropServer.Highlights;

public record Highlight
{
    [JsonPropertyName("_id")]
    public string? Id { get; init; }

    public string? Text { get; init; }

    public string? Title { get; init; }

    public string? Color { get; init; }

    public string? Note { get; init; }

    public string? Created { get; init; }

    public List<string>? Tags { get; init; }

    public string? Link { get; init; }

    public string? LastUpdate { get; init; }

    public long? RaindropRef { get; init; }
}
