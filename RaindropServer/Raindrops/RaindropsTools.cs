using System.ComponentModel;
using System.Linq;
using ModelContextProtocol.Server;
using RaindropTools.Common;

namespace RaindropTools.Raindrops;

[McpServerToolType]
public class RaindropsTools
{
    private readonly IRaindropsApi _api;

    public RaindropsTools(IRaindropsApi api)
    {
        _api = api;
    }

    [McpServerTool, Description("Create a new bookmark")]
    public Task<ItemResponse<Raindrop>> Create(int? collectionId, string url, string? title = null,
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
        return _api.CreateRaindrop(payload);
    }

    [McpServerTool, Description("Get a bookmark by id")]
    public Task<ItemResponse<Raindrop>> Get(long id) => _api.GetRaindrop(id);

    [McpServerTool, Description("Update an existing bookmark")]
    public Task<ItemResponse<Raindrop>> Update(long id, string? title = null, string? excerpt = null,
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
        return _api.UpdateRaindrop(id, payload);
    }

    [McpServerTool, Description("Delete a bookmark by id")]
    public Task<SuccessResponse> Delete(long id) => _api.DeleteRaindrop(id);

    [McpServerTool, Description("Search bookmarks in a collection")]
    public Task<ItemsResponse<Raindrop>> Search(int collectionId, string query) => _api.SearchRaindrops(collectionId, query);

    [McpServerTool, Description("Bulk update bookmarks in a collection")]
    public Task<SuccessResponse> UpdateMany(int collectionId, RaindropsBulkUpdate update, bool? nested = null, string? search = null)
        => _api.UpdateRaindrops(collectionId, update, nested, search);
}
