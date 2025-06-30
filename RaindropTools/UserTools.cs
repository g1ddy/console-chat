using System.ComponentModel;
using System.Net.Http;
using ModelContextProtocol.Server;

namespace RaindropTools;

[McpServerToolType]
public class UserTools
{
    private readonly RaindropApiClient _client;

    public UserTools(RaindropApiClient client)
    {
        _client = client;
    }

    [McpServerTool, Description("Get current user information")]
    public async Task<string> Get()
    {
        var response = await _client.SendAsync(HttpMethod.Get, "user");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    [McpServerTool, Description("Update current user profile")]
    public async Task<string> Update(string? email = null, string? name = null)
    {
        var payload = new { email, name };
        var response = await _client.SendAsync(HttpMethod.Put, "user", payload);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }
}
