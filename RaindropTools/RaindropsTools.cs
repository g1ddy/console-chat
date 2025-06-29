using System.ComponentModel;
using System.Net.Http;
using ModelContextProtocol.Server;

namespace RaindropTools;

[McpServerToolType]
public static class RaindropsTools
{
    [McpServerTool, Description("Create a new bookmark in the specified collection")]
    public static async Task<string> Create(string token, int collectionId, string url, string? title = null, string? excerpt = null)
    {
        var payload = new { link = url, title, excerpt };
        var response = await RaindropApiClient.SendAsync(HttpMethod.Post, $"raindrop/{collectionId}", token, payload);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    [McpServerTool, Description("Get a bookmark by id")]
    public static async Task<string> Get(string token, long id)
    {
        var response = await RaindropApiClient.SendAsync(HttpMethod.Get, $"raindrop/{id}", token);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    [McpServerTool, Description("Update an existing bookmark")]
    public static async Task<string> Update(string token, long id, string? title = null, string? excerpt = null, string? link = null)
    {
        var payload = new { link, title, excerpt };
        var response = await RaindropApiClient.SendAsync(HttpMethod.Put, $"raindrop/{id}", token, payload);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    [McpServerTool, Description("Delete a bookmark by id")]
    public static async Task<string> Delete(string token, long id)
    {
        var response = await RaindropApiClient.SendAsync(HttpMethod.Delete, $"raindrop/{id}", token);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    [McpServerTool, Description("Search bookmarks in a collection")]
    public static async Task<string> Search(string token, int collectionId, string query)
    {
        var url = $"raindrops/{collectionId}?search={Uri.EscapeDataString(query)}";
        var response = await RaindropApiClient.SendAsync(HttpMethod.Get, url, token);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }
}
