using System.ComponentModel;
using System.Linq;
using ModelContextProtocol.Server;
using RaindropServer.Common;

namespace RaindropServer.Raindrops;

[McpServerToolType]
public class RaindropsTools(IRaindropsApi api) :
    RaindropToolBase<IRaindropsApi>(api)
{

[McpServerTool(Title = "Create Bookmark"),
     Description("Creates a new bookmark.")]
    public Task<ItemResponse<Raindrop>> CreateBookmarkAsync(
        [Description("Bookmark creation details")] RaindropCreateRequest request)
    {
        var payload = request.ToRaindrop();
        return Api.CreateAsync(payload);
    }

[McpServerTool(Destructive = false, Idempotent = true, ReadOnly = true,
        Title = "Get Bookmark"),
     Description("Retrieves a single bookmark by its unique ID.")]
    public Task<ItemResponse<Raindrop>> GetBookmarkAsync([
        Description("ID of the bookmark to retrieve")] long id)
        => Api.GetAsync(id);

[McpServerTool(Idempotent = true, Title = "Update Bookmark"),
     Description("Updates an existing bookmark.")]
    public Task<ItemResponse<Raindrop>> UpdateBookmarkAsync(
        [Description("ID of the bookmark to update")] long id,
        [Description("Updated bookmark data")] RaindropUpdateRequest request)
    {
        var payload = request.ToRaindrop();
        return Api.UpdateAsync(id, payload);
    }

[McpServerTool(Idempotent = true, Title = "Delete Bookmark"),
     Description("Moves a bookmark to the Trash.")]
    public Task<SuccessResponse> DeleteBookmarkAsync([
        Description("ID of the bookmark to delete")] long id)
        => Api.DeleteAsync(id);


[McpServerTool(Destructive = false, Idempotent = true, ReadOnly = true,
        Title = "List Bookmarks"),
     Description("Retrieves a list of bookmarks from a specific collection.")]
    public Task<ItemsResponse<Raindrop>> ListBookmarksAsync(
        [Description("The ID of the collection to retrieve bookmarks from. Use 0 for all, -1 for unsorted, -99 for trash.")] int collectionId,
        [Description("A search query to filter the bookmarks.")] string? search = null)
        => Api.ListAsync(collectionId, search);

[McpServerTool(Title = "Create Bookmarks"),
     Description("Creates multiple bookmarks in a single request.")]
    public Task<ItemsResponse<Raindrop>> CreateBookmarksAsync(
        [Description("Collection ID for the new bookmarks")] int collectionId,
        [Description("A collection of bookmark details to create.")] IEnumerable<Raindrop> raindrops)
    {
        var payload = new RaindropCreateManyRequest { CollectionId = collectionId, Items = raindrops.ToList() };
        return Api.CreateManyAsync(payload);
    }


    [McpServerTool(Idempotent = true, Title = "Update Bookmarks"),
     Description("Bulk update bookmarks in a collection")]
    public Task<SuccessResponse> UpdateBookmarksAsync(
        [Description("Collection to update")] int collectionId,
        [Description("Update operations to apply")] RaindropBulkUpdate update,
        [Description("Apply to nested collections")] bool? nested = null,
        [Description("Optional search filter")] string? search = null)
        => Api.UpdateManyAsync(collectionId, update, nested, search);
}
