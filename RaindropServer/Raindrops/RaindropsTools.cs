using System.ComponentModel;
using System.Linq;
using ModelContextProtocol.Server;
using RaindropServer.Common;

namespace RaindropServer.Raindrops;

[McpServerToolType]
public class RaindropsTools(IRaindropsApi api) :
    RaindropToolBase<IRaindropsApi>(api)
{

    [McpServerTool, Description("Create a new bookmark")]
    public Task<ItemResponse<Raindrop>> CreateAsync(int? collectionId, string url, string? title = null,
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

    [McpServerTool, Description("Get a bookmark by id")]
    public Task<ItemResponse<Raindrop>> GetAsync(long id) => Api.GetAsync(id);

    [McpServerTool, Description("Update an existing bookmark")]
    public Task<ItemResponse<Raindrop>> UpdateAsync(long id, string? title = null, string? excerpt = null,
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

    [McpServerTool, Description("Delete a bookmark by id")]
    public Task<SuccessResponse> DeleteAsync(long id) => Api.DeleteAsync(id);


    [McpServerTool, Description("List bookmarks in a collection. Can be filtered by an optional search query.")]
    public Task<ItemsResponse<Raindrop>> ListAsync(int collectionId, string? search = null)
        => Api.ListAsync(collectionId, search);

    [McpServerTool, Description("Create multiple bookmarks")]
    public Task<ItemsResponse<Raindrop>> CreateManyAsync(int collectionId, IEnumerable<Raindrop> raindrops)
    {
        var payload = new RaindropCreateManyRequest { CollectionId = collectionId, Items = raindrops.ToList() };
        return Api.CreateManyAsync(payload);
    }


    [McpServerTool, Description("Bulk update bookmarks in a collection")]
    public Task<SuccessResponse> UpdateManyAsync(int collectionId, RaindropBulkUpdate update, bool? nested = null, string? search = null)
        => Api.UpdateManyAsync(collectionId, update, nested, search);
}
