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
    public Task<ItemsResponse<Highlight>> ListHighlightsAsync(
        [Description("Page number starting from 0")] int? page = null,
        [Description("Items per page")] int? perPage = null) =>
        Api.ListAsync(page, perPage);

    [McpServerTool(Destructive = false, Idempotent = true, ReadOnly = true,
        Title = "List Highlights By Collection"),
     Description("List highlights in a collection")]
    public Task<ItemsResponse<Highlight>> ListHighlightsByCollectionAsync(
        [Description("Collection ID containing the bookmarks")] int collectionId,
        [Description("Page number starting from 0")] int? page = null,
        [Description("Items per page")] int? perPage = null) =>
        Api.ListByCollectionAsync(collectionId, page, perPage);

    [McpServerTool(Destructive = false, Idempotent = true, ReadOnly = true,
        Title = "Get Bookmark Highlights"),
     Description("Get highlights for a bookmark")]
    public Task<ItemResponse<RaindropHighlights>> GetBookmarkHighlightsAsync([
        Description("ID of the bookmark") ] long raindropId) => Api.GetAsync(raindropId);

    [McpServerTool(Title = "Create Highlight"),
     Description("Create a highlight for a bookmark")]
    public Task<ItemResponse<HighlightBulkUpdateRequest>> CreateHighlightAsync(
        [Description("ID of the bookmark") ] long raindropId,
        [Description("Highlight creation details") ] HighlightCreateRequest request)
    {
        var payload = new HighlightBulkUpdateRequest
        {
            Highlights = [ new HighlightBulkUpdate { Text = request.Text, Color = request.Color, Note = request.Note } ]
        };
        return Api.UpdateAsync(raindropId, payload);
    }

    [McpServerTool(Idempotent = true, Title = "Update Highlight"),
     Description("Update an existing highlight")]
    public Task<ItemResponse<HighlightBulkUpdateRequest>> UpdateHighlightAsync(
        [Description("ID of the bookmark") ] long raindropId,
        [Description("Highlight update data") ] HighlightUpdateRequest request)
    {
        var payload = new HighlightBulkUpdateRequest
        {
            Highlights = [ new HighlightBulkUpdate { Id = request.Id, Text = request.Text, Color = request.Color, Note = request.Note } ]
        };
        return Api.UpdateAsync(raindropId, payload);
    }

    [McpServerTool(Idempotent = true, Title = "Delete Highlight"),
     Description("Remove a highlight by sending an empty text for that id")]
    public Task<ItemResponse<HighlightBulkUpdateRequest>> DeleteHighlightAsync(
        [Description("ID of the bookmark") ] long raindropId,
        [Description("ID of the highlight to remove") ] string highlightId)
    {
        var payload = new HighlightBulkUpdateRequest
        {
            Highlights = [ new HighlightBulkUpdate { Id = highlightId, Text = string.Empty } ]
        };
        return Api.UpdateAsync(raindropId, payload);
}
}
