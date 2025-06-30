using System.ComponentModel;
using System.Net.Http;
using System.Text.Json.Serialization;
using ModelContextProtocol.Server;

namespace RaindropTools;

[McpServerToolType]
public class RaindropsTools
{
    private readonly RaindropApiClient _client;

    public RaindropsTools(RaindropApiClient client)
    {
        _client = client;
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
        return await _client.SendAsync<ItemResponse<Raindrop>>(HttpMethod.Post, "raindrop", payload);
    }

    [McpServerTool, Description("Get a bookmark by id")]
    public async Task<ItemResponse<Raindrop>> Get(long id)
    {
        return await _client.SendAsync<ItemResponse<Raindrop>>(HttpMethod.Get, $"raindrop/{id}");
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
        return await _client.SendAsync<ItemResponse<Raindrop>>(HttpMethod.Put, $"raindrop/{id}", payload);
    }

    [McpServerTool, Description("Delete a bookmark by id")]
    public async Task<SuccessResponse> Delete(long id)
    {
        return await _client.SendAsync<SuccessResponse>(HttpMethod.Delete, $"raindrop/{id}");
    }

    [McpServerTool, Description("Search bookmarks in a collection")]
    public async Task<ItemsResponse<Raindrop>> Search(int collectionId, string query)
    {
        var url = $"raindrops/{collectionId}?search={Uri.EscapeDataString(query)}";
        return await _client.SendAsync<ItemsResponse<Raindrop>>(HttpMethod.Get, url);
    }
}

internal class IdRef
{
    [JsonPropertyName("$id")] public int Id { get; set; }
}
