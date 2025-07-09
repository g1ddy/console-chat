namespace RaindropServer.Highlights;

public record HighlightBulkUpdateRequest
{
    public List<HighlightBulkUpdate> Highlights { get; init; } = new();
}
