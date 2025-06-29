using System.ComponentModel;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using ModelContextProtocol.Server;

namespace RaindropTools;

internal static class RaindropApiHelpers
{
    public static HttpClient CreateClient(string token)
    {
        var client = new HttpClient { BaseAddress = new Uri("https://api.raindrop.io/rest/v1/") };
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }
}

[McpServerToolType]
public static class RaindropsTools
{
    [McpServerTool, Description("Create a new bookmark in the specified collection")]
    public static async Task<string> Create(string token, int collectionId, string url, string? title = null, string? excerpt = null)
    {
        using var client = RaindropApiHelpers.CreateClient(token);
        var payload = new { link = url, title, excerpt };
        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        var response = await client.PostAsync($"raindrop/{collectionId}", content);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    [McpServerTool, Description("Get a bookmark by id")]
    public static async Task<string> Get(string token, long id)
    {
        using var client = RaindropApiHelpers.CreateClient(token);
        var response = await client.GetAsync($"raindrop/{id}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    [McpServerTool, Description("Update an existing bookmark")]
    public static async Task<string> Update(string token, long id, string? title = null, string? excerpt = null, string? link = null)
    {
        using var client = RaindropApiHelpers.CreateClient(token);
        var payload = new { link, title, excerpt };
        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        var response = await client.PutAsync($"raindrop/{id}", content);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    [McpServerTool, Description("Delete a bookmark by id")]
    public static async Task<string> Delete(string token, long id)
    {
        using var client = RaindropApiHelpers.CreateClient(token);
        var response = await client.DeleteAsync($"raindrop/{id}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    [McpServerTool, Description("Search bookmarks in a collection")]
    public static async Task<string> Search(string token, int collectionId, string query)
    {
        using var client = RaindropApiHelpers.CreateClient(token);
        var url = $"raindrops/{collectionId}?search={Uri.EscapeDataString(query)}";
        var response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }
}

[McpServerToolType]
public static class HighlightsTools
{
    [McpServerTool, Description("Get highlights for a bookmark")]
    public static async Task<string> Get(string token, long raindropId)
    {
        using var client = RaindropApiHelpers.CreateClient(token);
        var response = await client.GetAsync($"raindrop/{raindropId}/highlights");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    [McpServerTool, Description("Create a highlight for a bookmark")]
    public static async Task<string> Create(string token, long raindropId, string text)
    {
        using var client = RaindropApiHelpers.CreateClient(token);
        var payload = new { text };
        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        var response = await client.PostAsync($"raindrop/{raindropId}/highlights", content);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    [McpServerTool, Description("Update an existing highlight")]
    public static async Task<string> Update(string token, long highlightId, string text)
    {
        using var client = RaindropApiHelpers.CreateClient(token);
        var payload = new { text };
        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        var response = await client.PutAsync($"highlight/{highlightId}", content);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    [McpServerTool, Description("Delete a highlight by id")]
    public static async Task<string> Delete(string token, long highlightId)
    {
        using var client = RaindropApiHelpers.CreateClient(token);
        var response = await client.DeleteAsync($"highlight/{highlightId}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }
}

[McpServerToolType]
public static class UserTools
{
    [McpServerTool, Description("Get current user information")]
    public static async Task<string> Get(string token)
    {
        using var client = RaindropApiHelpers.CreateClient(token);
        var response = await client.GetAsync("user");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    [McpServerTool, Description("Update current user profile")]
    public static async Task<string> Update(string token, string? email = null, string? name = null)
    {
        using var client = RaindropApiHelpers.CreateClient(token);
        var payload = new { email, name };
        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        var response = await client.PutAsync("user", content);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }
}

[McpServerToolType]
public static class TagsTools
{
    [McpServerTool, Description("List all tags")]
    public static async Task<string> List(string token)
    {
        using var client = RaindropApiHelpers.CreateClient(token);
        var response = await client.GetAsync("tags");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    [McpServerTool, Description("Rename a tag")]
    public static async Task<string> Rename(string token, string oldTag, string newTag)
    {
        using var client = RaindropApiHelpers.CreateClient(token);
        var payload = new { newName = newTag };
        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        var response = await client.PutAsync($"tag/{Uri.EscapeDataString(oldTag)}", content);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    [McpServerTool, Description("Delete a tag")]
    public static async Task<string> Delete(string token, string tag)
    {
        using var client = RaindropApiHelpers.CreateClient(token);
        var response = await client.DeleteAsync($"tag/{Uri.EscapeDataString(tag)}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }
}
