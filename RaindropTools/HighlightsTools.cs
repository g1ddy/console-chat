using System.ComponentModel;
using System.Net.Http;
using ModelContextProtocol.Server;

namespace RaindropTools;

[McpServerToolType]
public static class HighlightsTools
{
    [McpServerTool, Description("Get highlights for a bookmark")]
    public static async Task<string> Get(string token, long raindropId)
    {
        var response = await RaindropApiClient.SendAsync(HttpMethod.Get, $"raindrop/{raindropId}/highlights", token);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    [McpServerTool, Description("Create a highlight for a bookmark")]
    public static async Task<string> Create(string token, long raindropId, string text)
    {
        var payload = new { text };
        var response = await RaindropApiClient.SendAsync(HttpMethod.Post, $"raindrop/{raindropId}/highlights", token, payload);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    [McpServerTool, Description("Update an existing highlight")]
    public static async Task<string> Update(string token, long highlightId, string text)
    {
        var payload = new { text };
        var response = await RaindropApiClient.SendAsync(HttpMethod.Put, $"highlight/{highlightId}", token, payload);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    [McpServerTool, Description("Delete a highlight by id")]
    public static async Task<string> Delete(string token, long highlightId)
    {
        var response = await RaindropApiClient.SendAsync(HttpMethod.Delete, $"highlight/{highlightId}", token);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }
}
