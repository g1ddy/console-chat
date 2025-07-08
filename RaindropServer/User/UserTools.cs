using System.ComponentModel;
using ModelContextProtocol.Server;
using RaindropServer.Common;

namespace RaindropServer.User;

[McpServerToolType]
public class UserTools(IUserApi api) : RaindropToolBase<IUserApi>(api)
{

    [McpServerTool, Description("Get current user information")]
    public Task<ItemResponse<UserInfo>> GetAsync() => Api.GetAsync();
}
