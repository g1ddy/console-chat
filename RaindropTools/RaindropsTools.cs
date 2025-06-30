using System.ComponentModel;
using Refit;
using System.Text.Json.Serialization;
using ModelContextProtocol.Server;

namespace RaindropTools;

[McpServerToolType]
public class RaindropsTools
{
    private readonly IRaindropApi _api;

    public RaindropsTools(IRaindropApi api)
    {
        _api = api;
    }

    [McpServerTool, Description("Create a new bookmark in the specified collection")]
    public async Task<ItemResponse<Raindrop>> Create(int collectionId, string url, string? title = null,
        string? excerpt = null, IEnumerable<string>? tags = null, bool? important = null)
    {
        var payload = new
        {
            link = url,
            title,
            excerpt,
            tags,
            important,
            collection = new IdRef { Id = collectionId }
        };
        return await _api.CreateRaindrop(payload);
    }

    [McpServerTool, Description("Get a bookmark by id")]
    public async Task<ItemResponse<Raindrop>> Get(long id)
    {
        return await _api.GetRaindrop(id);
    }

    [McpServerTool, Description("Update an existing bookmark")]
    public async Task<ItemResponse<Raindrop>> Update(long id, string? title = null, string? excerpt = null,
        string? link = null, IEnumerable<string>? tags = null, bool? important = null,
        int? collectionId = null)
    {
        var payload = new
        {
            link,
            title,
            excerpt,
            tags,
            important,
            collectionId
        };
        return await _api.UpdateRaindrop(id, payload);
    }

    [McpServerTool, Description("Delete a bookmark by id")]
    public async Task<SuccessResponse> Delete(long id)
    {
        return await _api.DeleteRaindrop(id);
    }

    [McpServerTool, Description("Search bookmarks in a collection")]
    public async Task<ItemsResponse<Raindrop>> Search(int collectionId, string query)
    {
        return await _api.SearchRaindrops(collectionId, query);
    }
}

internal class IdRef
{
    [JsonPropertyName("$id")] public int Id { get; set; }
}
