using System.ComponentModel;
using System.Net.Http;
using ModelContextProtocol.Server;

namespace RaindropTools;

[McpServerToolType]
public static class TagsTools
{
    [McpServerTool, Description("List all tags")]
    public static async Task<string> List(string token)
    {
        var response = await RaindropApiClient.SendAsync(HttpMethod.Get, "tags", token);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    [McpServerTool, Description("Rename a tag")]
    public static async Task<string> Rename(string token, string oldTag, string newTag)
    {
        var payload = new { newName = newTag };
        var response = await RaindropApiClient.SendAsync(HttpMethod.Put, $"tag/{Uri.EscapeDataString(oldTag)}", token, payload);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    [McpServerTool, Description("Delete a tag")]
    public static async Task<string> Delete(string token, string tag)
    {
        var response = await RaindropApiClient.SendAsync(HttpMethod.Delete, $"tag/{Uri.EscapeDataString(tag)}", token);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }
}
