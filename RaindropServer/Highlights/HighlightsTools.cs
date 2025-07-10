using System.ComponentModel;
using ModelContextProtocol.Server;
using RaindropServer.Common;

namespace RaindropServer.Highlights;

[McpServerToolType]
public class HighlightsTools(IHighlightsApi api) : RaindropToolBase<IHighlightsApi>(api)
{
    [McpServerTool(Destructive = false, Idempotent = true, ReadOnly = true,
        Title = "List Highlights"),
     Description("List all highlights")]
    public Task<ItemsResponse<Highlight>> ListHighlightsAsync(int? page = null, int? perPage = null) =>
        Api.ListAsync(page, perPage);

    [McpServerTool(Destructive = false, Idempotent = true, ReadOnly = true,
        Title = "List Highlights By Collection"),
     Description("List highlights in a collection")]
    public Task<ItemsResponse<Highlight>> ListHighlightsByCollectionAsync(int collectionId, int? page = null, int? perPage = null) =>
        Api.ListByCollectionAsync(collectionId, page, perPage);

    [McpServerTool(Destructive = false, Idempotent = true, ReadOnly = true,
        Title = "Get Bookmark Highlights"),
     Description("Get highlights for a bookmark")]
    public Task<ItemResponse<RaindropHighlights>> GetBookmarkHighlightsAsync(long raindropId) => Api.GetAsync(raindropId);

    [McpServerTool(Title = "Create Highlight"),
     Description("Create a highlight for a bookmark")]
    public Task<ItemResponse<HighlightBulkUpdateRequest>> CreateHighlightAsync(long raindropId, HighlightCreateRequest request)
    {
        var payload = new HighlightBulkUpdateRequest
        {
            Highlights = [ new HighlightBulkUpdate { Text = request.Text, Color = request.Color, Note = request.Note } ]
        };
        return Api.UpdateAsync(raindropId, payload);
    }

    [McpServerTool(Idempotent = true, Title = "Update Highlight"),
     Description("Update an existing highlight")]
    public Task<ItemResponse<HighlightBulkUpdateRequest>> UpdateHighlightAsync(long raindropId, HighlightUpdateRequest request)
    {
        var payload = new HighlightBulkUpdateRequest
        {
            Highlights = [ new HighlightBulkUpdate { Id = request.Id, Text = request.Text, Color = request.Color, Note = request.Note } ]
        };
        return Api.UpdateAsync(raindropId, payload);
    }

    [McpServerTool(Idempotent = true, Title = "Delete Highlight"),
     Description("Remove a highlight by sending an empty text for that id")]
    public Task<ItemResponse<HighlightBulkUpdateRequest>> DeleteHighlightAsync(long raindropId, string highlightId)
    {
        var payload = new HighlightBulkUpdateRequest
        {
            Highlights = [ new HighlightBulkUpdate { Id = highlightId, Text = string.Empty } ]
        };
        return Api.UpdateAsync(raindropId, payload);
}
}
