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
     Description("Create a new bookmark")]
    public Task<ItemResponse<Raindrop>> CreateBookmarkAsync(int? collectionId, string url, string? title = null,
        string? excerpt = null, IEnumerable<string>? tags = null, bool? important = null)
    {
        var payload = new Raindrop
        {
            Link = url,
            Title = title,
            Excerpt = excerpt,
            Tags = tags?.ToList(),
            Important = important,
            CollectionId = collectionId
        };
        return Api.CreateAsync(payload);
    }

    [McpServerTool(Destructive = false, Idempotent = true, ReadOnly = true,
        Title = "Get Bookmark"),
     Description("Get a bookmark by id")]
    public Task<ItemResponse<Raindrop>> GetBookmarkAsync(long id) => Api.GetAsync(id);

    [McpServerTool(Idempotent = true, Title = "Update Bookmark"),
     Description("Update an existing bookmark")]
    public Task<ItemResponse<Raindrop>> UpdateBookmarkAsync(long id, string? title = null, string? excerpt = null,
        string? link = null, IEnumerable<string>? tags = null, bool? important = null,
        int? collectionId = null)
    {
        var payload = new Raindrop
        {
            Link = link,
            Title = title,
            Excerpt = excerpt,
            Tags = tags?.ToList(),
            Important = important,
            CollectionId = collectionId
        };
        return Api.UpdateAsync(id, payload);
    }

    [McpServerTool(Idempotent = true, Title = "Delete Bookmark"),
     Description("Delete a bookmark by id")]
    public Task<SuccessResponse> DeleteBookmarkAsync(long id) => Api.DeleteAsync(id);


    [McpServerTool(Destructive = false, Idempotent = true, ReadOnly = true,
        Title = "List Bookmarks"),
     Description("List bookmarks in a collection. Can be filtered by an optional search query.")]
    public Task<ItemsResponse<Raindrop>> ListBookmarksAsync(int collectionId, string? search = null)
        => Api.ListAsync(collectionId, search);

    [McpServerTool(Title = "Create Bookmarks"),
     Description("Create multiple bookmarks")]
    public Task<ItemsResponse<Raindrop>> CreateBookmarksAsync(int collectionId, IEnumerable<Raindrop> raindrops)
    {
        var payload = new RaindropCreateManyRequest { CollectionId = collectionId, Items = raindrops.ToList() };
        return Api.CreateManyAsync(payload);
    }


    [McpServerTool(Idempotent = true, Title = "Update Bookmarks"),
     Description("Bulk update bookmarks in a collection")]
    public Task<SuccessResponse> UpdateBookmarksAsync(int collectionId, RaindropBulkUpdate update, bool? nested = null, string? search = null)
        => Api.UpdateManyAsync(collectionId, update, nested, search);
}
