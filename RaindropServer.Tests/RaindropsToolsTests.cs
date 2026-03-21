using NSubstitute;
using RaindropServer.Raindrops;
using Xunit;

namespace RaindropServer.Tests;

public class RaindropsToolsTests
{
    private readonly IRaindropsApi _api;
    private readonly RaindropsTools _sut;

    public RaindropsToolsTests()
    {
        _api = Substitute.For<IRaindropsApi>();
        _sut = new RaindropsTools(_api);
    }

    [Fact]
    public async Task ListBookmarksAsync_NegativePage_ThrowsArgumentOutOfRangeException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            _sut.ListBookmarksAsync(0, page: -1));
        Assert.Equal("page", exception.ParamName);
    }

    [Fact]
    public async Task ListBookmarksAsync_PerPageLessThanOne_ThrowsArgumentOutOfRangeException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            _sut.ListBookmarksAsync(0, perPage: 0));
        Assert.Equal("perPage", exception.ParamName);
    }

    [Fact]
    public async Task ListBookmarksAsync_PerPageGreaterThanFifty_ThrowsArgumentOutOfRangeException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            _sut.ListBookmarksAsync(0, perPage: 51));
        Assert.Equal("perPage", exception.ParamName);
    }

    [Fact]
    public async Task ListBookmarksAsync_InvalidSort_ThrowsArgumentOutOfRangeException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            _sut.ListBookmarksAsync(0, sort: "invalid_sort"));
        Assert.Equal("sort", exception.ParamName);
    }

    [Fact]
    public async Task ListBookmarksAsync_SortScoreWithoutSearch_ThrowsArgumentException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _sut.ListBookmarksAsync(0, sort: "score", search: null));
        Assert.Equal("sort", exception.ParamName);
    }

    [Fact]
    public async Task ListBookmarksAsync_ValidParameters_DelegatesToApi()
    {
        // Arrange
        int collectionId = 123;
        string search = "test";
        string sort = "title";
        int page = 1;
        int perPage = 20;
        bool nested = true;

        // Act
        await _sut.ListBookmarksAsync(collectionId, search, sort, page, perPage, nested);

        // Assert
        await _api.Received(1).ListAsync(collectionId, search, sort, page, perPage, nested);
    }
}
