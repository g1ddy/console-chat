using System.ComponentModel;
using System.Net.Http;
using ModelContextProtocol.Server;

namespace RaindropTools;

[McpServerToolType]
public static class UserTools
{
    [McpServerTool, Description("Get current user information")]
    public static async Task<string> Get(string token)
    {
        var response = await RaindropApiClient.SendAsync(HttpMethod.Get, "user", token);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    [McpServerTool, Description("Update current user profile")]
    public static async Task<string> Update(string token, string? email = null, string? name = null)
    {
        var payload = new { email, name };
        var response = await RaindropApiClient.SendAsync(HttpMethod.Put, "user", token, payload);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }
}
