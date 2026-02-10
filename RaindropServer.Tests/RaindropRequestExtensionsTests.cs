using RaindropServer.Raindrops;
using Xunit;

namespace RaindropServer.Tests;

public class RaindropRequestExtensionsTests
{
    [Fact]
    public void ToRaindrop_CreateRequest_MapsAllProperties()
    {
        // Arrange
        var request = new RaindropCreateRequest
        {
            Link = "https://example.com",
            Title = "Test Title",
            Excerpt = "Test Excerpt",
            Note = "Test Note",
            Tags = new List<string> { "tag1", "tag2" },
            Important = true,
            CollectionId = 123
        };

        // Act
        var raindrop = request.ToRaindrop();

        // Assert
        Assert.Equal(request.Link, raindrop.Link);
        Assert.Equal(request.Title, raindrop.Title);
        Assert.Equal(request.Excerpt, raindrop.Excerpt);
        Assert.Equal(request.Note, raindrop.Note);
        Assert.NotNull(raindrop.Tags);
        Assert.Equal(request.Tags, raindrop.Tags);
        Assert.Equal(request.Important, raindrop.Important);
        Assert.Equal(request.CollectionId, raindrop.CollectionId);
    }

    [Fact]
    public void ToRaindrop_CreateRequest_HandlesNullTags()
    {
        // Arrange
        var request = new RaindropCreateRequest
        {
            Link = "https://example.com",
            Tags = null
        };

        // Act
        var raindrop = request.ToRaindrop();

        // Assert
        Assert.Null(raindrop.Tags);
    }

    [Fact]
    public void ToRaindrop_CreateRequest_HandlesEmptyTags()
    {
        // Arrange
        var request = new RaindropCreateRequest
        {
            Link = "https://example.com",
            Tags = new List<string>()
        };

        // Act
        var raindrop = request.ToRaindrop();

        // Assert
        Assert.NotNull(raindrop.Tags);
        Assert.Empty(raindrop.Tags);
    }

    [Fact]
    public void ToRaindrop_UpdateRequest_MapsAllProperties()
    {
        // Arrange
        var request = new RaindropUpdateRequest
        {
            Link = "https://updated.example.com",
            Title = "Updated Title",
            Excerpt = "Updated Excerpt",
            Note = "Updated Note",
            Tags = new List<string> { "updatedTag1", "updatedTag2" },
            Important = false,
            CollectionId = 456
        };

        // Act
        var raindrop = request.ToRaindrop();

        // Assert
        Assert.Equal(request.Link, raindrop.Link);
        Assert.Equal(request.Title, raindrop.Title);
        Assert.Equal(request.Excerpt, raindrop.Excerpt);
        Assert.Equal(request.Note, raindrop.Note);
        Assert.NotNull(raindrop.Tags);
        Assert.Equal(request.Tags, raindrop.Tags);
        Assert.Equal(request.Important, raindrop.Important);
        Assert.Equal(request.CollectionId, raindrop.CollectionId);
    }

    [Fact]
    public void ToRaindrop_UpdateRequest_HandlesNullTags()
    {
        // Arrange
        var request = new RaindropUpdateRequest
        {
            Tags = null
        };

        // Act
        var raindrop = request.ToRaindrop();

        // Assert
        Assert.Null(raindrop.Tags);
    }

    [Fact]
    public void ToRaindrop_UpdateRequest_HandlesPartialUpdate()
    {
        // Arrange
        var request = new RaindropUpdateRequest
        {
            Title = "Partial Update Title"
        };

        // Act
        var raindrop = request.ToRaindrop();

        // Assert
        Assert.Equal(request.Title, raindrop.Title);
        Assert.Null(raindrop.Link);
        Assert.Null(raindrop.Excerpt);
        Assert.Null(raindrop.Note);
        Assert.Null(raindrop.Tags);
        Assert.Null(raindrop.Important);
        Assert.Null(raindrop.CollectionId);
    }
}
