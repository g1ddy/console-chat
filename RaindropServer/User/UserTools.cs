using System.ComponentModel;
using ModelContextProtocol.Server;
using RaindropServer.Common;

namespace RaindropServer.User;

[McpServerToolType]
public class UserTools
{
    private readonly IUserApi _api;

    public UserTools(IUserApi api)
    {
        _api = api;
    }

    [McpServerTool, Description("Get current user information")]
    public Task<ItemResponse<UserInfo>> GetAsync() => _api.GetAsync();
}
