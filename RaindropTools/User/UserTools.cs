using System.ComponentModel;
using ModelContextProtocol.Server;

namespace RaindropTools.User;

[McpServerToolType]
public class UserTools
{
    private readonly IRaindropApi _api;

    public UserTools(IRaindropApi api)
    {
        _api = api;
    }

    [McpServerTool, Description("Get current user information")]
    public Task<string> Get() => _api.GetUser();

    [McpServerTool, Description("Update current user profile")]
    public Task<string> Update(string? email = null, string? name = null)
    {
        var payload = new { email, name };
        return _api.UpdateUser(payload);
    }
}
