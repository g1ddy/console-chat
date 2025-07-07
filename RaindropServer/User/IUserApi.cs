using Refit;
using RaindropServer.Common;

namespace RaindropServer.User;

public interface IUserApi
{
    [Get("/user")]
    Task<ItemResponse<UserInfo>> GetUserAsync();
}
