using System.ComponentModel;
using System.Linq;
using ModelContextProtocol.Server;
using RaindropServer.Common;

namespace RaindropServer.Raindrops;

[McpServerToolType]
public class RaindropsTools
{
    private readonly IRaindropsApi _api;

    public RaindropsTools(IRaindropsApi api)
    {
        _api = api;
    }

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
        return _api.CreateRaindropAsync(payload);
    }

    [McpServerTool, Description("Get a bookmark by id")]
    public Task<ItemResponse<Raindrop>> GetAsync(long id) => _api.GetRaindropAsync(id);

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
        return _api.UpdateRaindropAsync(id, payload);
    }

    [McpServerTool, Description("Delete a bookmark by id")]
    public Task<SuccessResponse> DeleteAsync(long id) => _api.DeleteRaindropAsync(id);

    [McpServerTool, Description("List bookmarks in a collection")]
    public Task<ItemsResponse<Raindrop>> ListAsync(int collectionId, string? search = null)
        => _api.GetRaindropsAsync(collectionId, search);

    [McpServerTool, Description("Create multiple bookmarks")]
    public Task<ItemsResponse<Raindrop>> CreateManyAsync(int collectionId, IEnumerable<Raindrop> raindrops)
    {
        var payload = new RaindropsCreateMany { CollectionId = collectionId, Items = raindrops.ToList() };
        return _api.CreateRaindropsAsync(payload);
    }

    [McpServerTool, Description("Search bookmarks in a collection")]
    public Task<ItemsResponse<Raindrop>> SearchAsync(int collectionId, string query) => _api.GetRaindropsAsync(collectionId, query);

    [McpServerTool, Description("Bulk update bookmarks in a collection")]
    public Task<SuccessResponse> UpdateManyAsync(int collectionId, RaindropsBulkUpdate update, bool? nested = null, string? search = null)
        => _api.UpdateRaindropsAsync(collectionId, update, nested, search);
}
