using System.ComponentModel;
using System.Net.Http;
using ModelContextProtocol.Server;

namespace RaindropTools;

[McpServerToolType]
public class HighlightsTools
{
    private readonly RaindropApiClient _client;

    public HighlightsTools(RaindropApiClient client)
    {
        _client = client;
    }

    [McpServerTool, Description("Get highlights for a bookmark")]
    public async Task<string> Get(long raindropId)
    {
        var response = await _client.SendAsync(HttpMethod.Get, $"raindrop/{raindropId}/highlights");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    [McpServerTool, Description("Create a highlight for a bookmark")]
    public async Task<string> Create(long raindropId, string text)
    {
        var payload = new { text };
        var response = await _client.SendAsync(HttpMethod.Post, $"raindrop/{raindropId}/highlights", payload);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    [McpServerTool, Description("Update an existing highlight")]
    public async Task<string> Update(long highlightId, string text)
    {
        var payload = new { text };
        var response = await _client.SendAsync(HttpMethod.Put, $"highlight/{highlightId}", payload);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    [McpServerTool, Description("Delete a highlight by id")]
    public async Task<string> Delete(long highlightId)
    {
        var response = await _client.SendAsync(HttpMethod.Delete, $"highlight/{highlightId}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }
}
