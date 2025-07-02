using System.ComponentModel;
using ModelContextProtocol.Server;
using RaindropTools.Common;

namespace RaindropTools.Highlights;

[McpServerToolType]
public class HighlightsTools
{
    private readonly IHighlightsApi _api;

    public HighlightsTools(IHighlightsApi api)
    {
        _api = api;
    }

    [McpServerTool, Description("List all highlights")]
    public Task<ItemsResponse<Highlight>> List(int? page = null, int? perPage = null) =>
        _api.ListHighlights(page, perPage);

    [McpServerTool, Description("List highlights in a collection")]
    public Task<ItemsResponse<Highlight>> ListByCollection(int collectionId, int? page = null, int? perPage = null) =>
        _api.ListHighlightsByCollection(collectionId, page, perPage);

    [McpServerTool, Description("Get highlights for a bookmark")]
    public Task<ItemResponse<RaindropHighlights>> Get(long raindropId) => _api.GetHighlights(raindropId);

    [McpServerTool, Description("Create a highlight for a bookmark")]
    public Task<ItemResponse<RaindropHighlights>> Create(long raindropId, string text, string? color = null, string? note = null)
    {
        var payload = new HighlightsBulkUpdate
        {
            Highlights = [ new Highlight { Text = text, Color = color, Note = note } ]
        };
        return _api.UpdateHighlights(raindropId, payload);
    }

    [McpServerTool, Description("Update an existing highlight")]
    public Task<ItemResponse<RaindropHighlights>> Update(long raindropId, string highlightId, string? text = null, string? color = null, string? note = null)
    {
        var payload = new HighlightsBulkUpdate
        {
            Highlights = [ new Highlight { Id = highlightId, Text = text, Color = color, Note = note } ]
        };
        return _api.UpdateHighlights(raindropId, payload);
    }

    [McpServerTool, Description("Remove a highlight by sending an empty text for that id")]
    public Task<ItemResponse<RaindropHighlights>> Delete(long raindropId, string highlightId)
    {
        var payload = new HighlightsBulkUpdate
        {
            Highlights = [ new Highlight { Id = highlightId, Text = string.Empty } ]
        };
        return _api.UpdateHighlights(raindropId, payload);
    }
}
