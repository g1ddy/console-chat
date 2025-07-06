using System.ComponentModel;
using ModelContextProtocol.Server;
using RaindropTools.Common;

namespace RaindropTools.User;

[McpServerToolType]
public class UserTools
{
    private readonly IUserApi _api;

    public UserTools(IUserApi api)
    {
        _api = api;
    }

    [McpServerTool, Description("Get current user information")]
    public Task<ItemResponse<UserInfo>> Get() => _api.GetUser();
}
