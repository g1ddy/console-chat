using NSubstitute;
using NSubstitute.ExceptionExtensions;
using RaindropServer.Common;
using RaindropServer.User;
using Xunit;

namespace RaindropServer.Tests;

public class UserToolsTests
{
    private readonly IUserApi _api;
    private readonly UserTools _sut;

    public UserToolsTests()
    {
        _api = Substitute.For<IUserApi>();
        _sut = new UserTools(_api);
    }

    [Fact]
    public async Task GetUserInfoAsync_DelegatesToApi()
    {
        // Arrange
        var expectedResponse = new ItemResponse<UserInfo>(true, new UserInfo
        {
            Id = 123,
            FullName = "Test User",
            Email = "test@example.com",
            Pro = true,
            Type = "pro"
        });

        _api.GetAsync().Returns(Task.FromResult(expectedResponse));

        // Act
        var result = await _sut.GetUserInfoAsync();

        // Assert
        Assert.Same(expectedResponse, result);
        await _api.Received(1).GetAsync();
    }

    [Fact]
    public async Task GetUserInfoAsync_PropagatesException()
    {
        // Arrange
        var expectedException = new InvalidOperationException("API Error");
        _api.GetAsync().Throws(expectedException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.GetUserInfoAsync());
        Assert.Same(expectedException, exception);
    }
}
