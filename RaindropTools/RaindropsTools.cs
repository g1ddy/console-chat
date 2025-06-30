using System.ComponentModel;
using System.Net.Http;
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
    public async Task<string> Create(int collectionId, string url, string? title = null, string? excerpt = null)
    {
        var payload = new { link = url, title, excerpt };
        var response = await _client.SendAsync(HttpMethod.Post, $"raindrop/{collectionId}", payload);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    [McpServerTool, Description("Get a bookmark by id")]
    public async Task<string> Get(long id)
    {
        var response = await _client.SendAsync(HttpMethod.Get, $"raindrop/{id}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    [McpServerTool, Description("Update an existing bookmark")]
    public async Task<string> Update(long id, string? title = null, string? excerpt = null, string? link = null)
    {
        var payload = new { link, title, excerpt };
        var response = await _client.SendAsync(HttpMethod.Put, $"raindrop/{id}", payload);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    [McpServerTool, Description("Delete a bookmark by id")]
    public async Task<string> Delete(long id)
    {
        var response = await _client.SendAsync(HttpMethod.Delete, $"raindrop/{id}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    [McpServerTool, Description("Search bookmarks in a collection")]
    public async Task<string> Search(int collectionId, string query)
    {
        var url = $"raindrops/{collectionId}?search={Uri.EscapeDataString(query)}";
        var response = await _client.SendAsync(HttpMethod.Get, url);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }
}
