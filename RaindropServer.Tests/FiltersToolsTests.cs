using NSubstitute;
using RaindropServer.Filters;

namespace RaindropServer.Tests;

public class FiltersToolsTests
{
    private readonly IFiltersApi _api;
    private readonly FiltersTools _sut;

    public FiltersToolsTests()
    {
        _api = Substitute.For<IFiltersApi>();
        _sut = new FiltersTools(_api);
    }

    [Fact]
    public async Task GetAvailableFiltersAsync_InvalidTagsSort_ThrowsArgumentOutOfRangeException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            _sut.GetAvailableFiltersAsync(0, tagsSort: "invalid"));
        Assert.Equal("tagsSort", exception.ParamName);
    }

    [Theory]
    [InlineData("-count")]
    [InlineData("_id")]
    [InlineData(null)]
    public async Task GetAvailableFiltersAsync_ValidTagsSort_CallsApiWithCorrectParameters(string? tagsSort)
    {
        // Arrange
        long collectionId = 123;
        string? search = "query";
        var expectedResponse = new AvailableFilters { Result = true };
        _api.GetAsync(collectionId, tagsSort, search).Returns(expectedResponse);

        // Act
        var result = await _sut.GetAvailableFiltersAsync(collectionId, tagsSort, search);

        // Assert
        Assert.Same(expectedResponse, result);
        await _api.Received(1).GetAsync(collectionId, tagsSort, search);
    }

    [Fact]
    public async Task GetAvailableFiltersAsync_WithDefaultParameters_CallsApiWithCorrectParameters()
    {
        // Arrange
        long collectionId = 456;
        var expectedResponse = new AvailableFilters { Result = true };
        _api.GetAsync(collectionId, null, null).Returns(expectedResponse);

        // Act
        var result = await _sut.GetAvailableFiltersAsync(collectionId);

        // Assert
        Assert.Same(expectedResponse, result);
        await _api.Received(1).GetAsync(collectionId, null, null);
    }
}
