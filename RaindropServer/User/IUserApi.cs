using Refit;
using RaindropTools.Common;

namespace RaindropTools.User;

public interface IUserApi
{
    [Get("/user")]
    Task<ItemResponse<UserInfo>> GetUser();
}
