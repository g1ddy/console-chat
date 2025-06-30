using System.ComponentModel;
using System.Net.Http;
using ModelContextProtocol.Server;

namespace RaindropTools;

[McpServerToolType]
public class TagsTools
{
    private readonly RaindropApiClient _client;

    public TagsTools(RaindropApiClient client)
    {
        _client = client;
    }

    [McpServerTool, Description("List all tags")]
    public async Task<string> List()
    {
        var response = await _client.SendAsync(HttpMethod.Get, "tags");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    [McpServerTool, Description("Rename a tag")]
    public async Task<string> Rename(string oldTag, string newTag)
    {
        var payload = new { newName = newTag };
        var response = await _client.SendAsync(HttpMethod.Put, $"tag/{Uri.EscapeDataString(oldTag)}", payload);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    [McpServerTool, Description("Delete a tag")]
    public async Task<string> Delete(string tag)
    {
        var response = await _client.SendAsync(HttpMethod.Delete, $"tag/{Uri.EscapeDataString(tag)}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }
}
