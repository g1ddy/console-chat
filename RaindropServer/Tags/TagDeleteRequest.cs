namespace RaindropServer.Tags;

public record TagDeleteRequest
{
    public List<string> Tags { get; init; } = new();
}
