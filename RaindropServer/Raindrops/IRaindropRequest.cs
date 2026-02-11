namespace RaindropServer.Raindrops;

/// <summary>
/// Interface defining common properties for Raindrop requests.
/// </summary>
public interface IRaindropRequest
{
    string? Link { get; }
    string? Title { get; }
    string? Excerpt { get; }
    string? Note { get; }
    IEnumerable<string>? Tags { get; }
    bool? Important { get; }
    int? CollectionId { get; }
}
