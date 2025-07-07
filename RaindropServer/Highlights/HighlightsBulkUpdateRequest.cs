namespace RaindropServer.Highlights;

public record HighlightsBulkUpdateRequest
{
    public List<HighlightsBulkUpdate> Highlights { get; init; } = new();
}
