using System.ComponentModel;
using Refit;
using ModelContextProtocol.Server;

namespace RaindropTools;

[McpServerToolType]
public class UserTools
{
    private readonly IRaindropApi _api;

    public UserTools(IRaindropApi api)
    {
        _api = api;
    }

    [McpServerTool, Description("Get current user information")]
    public async Task<string> Get()
    {
        return await _api.GetUser();
    }

    [McpServerTool, Description("Update current user profile")]
    public async Task<string> Update(string? email = null, string? name = null)
    {
        var payload = new { email, name };
        return await _api.UpdateUser(payload);
    }
}
