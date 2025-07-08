using System.ComponentModel;
using ModelContextProtocol.Server;
using RaindropServer.Common;

namespace RaindropServer.Highlights;

[McpServerToolType]
public class HighlightsTools(IHighlightsApi api) : RaindropToolBase<IHighlightsApi>(api)
{

    [McpServerTool, Description("List all highlights")]
    public Task<ItemsResponse<Highlight>> ListAsync(int? page = null, int? perPage = null) =>
        Api.ListAsync(page, perPage);

    [McpServerTool, Description("List highlights in a collection")]
    public Task<ItemsResponse<Highlight>> ListByCollectionAsync(int collectionId, int? page = null, int? perPage = null) =>
        Api.ListByCollectionAsync(collectionId, page, perPage);

    [McpServerTool, Description("Get highlights for a bookmark")]
    public Task<ItemResponse<RaindropHighlights>> GetAsync(long raindropId) => Api.GetAsync(raindropId);

    [McpServerTool, Description("Create a highlight for a bookmark")]
    public Task<ItemResponse<HighlightBulkUpdateRequest>> CreateAsync(long raindropId, string text, string? color = null, string? note = null)
    {
        var payload = new HighlightBulkUpdateRequest
        {
            Highlights = [ new HighlightBulkUpdate { Text = text, Color = color, Note = note } ]
        };
        return Api.UpdateAsync(raindropId, payload);
    }

    [McpServerTool, Description("Update an existing highlight")]
    public Task<ItemResponse<HighlightBulkUpdateRequest>> UpdateAsync(long raindropId, string highlightId, string? text = null, string? color = null, string? note = null)
    {
        var payload = new HighlightBulkUpdateRequest
        {
            Highlights = [ new HighlightBulkUpdate { Id = highlightId, Text = text, Color = color, Note = note } ]
        };
        return Api.UpdateAsync(raindropId, payload);
    }

    [McpServerTool, Description("Remove a highlight by sending an empty text for that id")]
    public Task<ItemResponse<HighlightBulkUpdateRequest>> DeleteAsync(long raindropId, string highlightId)
    {
        var payload = new HighlightBulkUpdateRequest
        {
            Highlights = [ new HighlightBulkUpdate { Id = highlightId, Text = string.Empty } ]
        };
        return Api.UpdateAsync(raindropId, payload);
}
}
