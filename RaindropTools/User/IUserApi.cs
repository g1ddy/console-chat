using Refit;

namespace RaindropTools.User;

public interface IUserApi
{
    [Get("/user")]
    Task<string> GetUser();

    [Put("/user")]
    Task<string> UpdateUser([Body] object payload);
}
