using RaindropServer.Collections;
using RaindropServer.Raindrops;
using RaindropServer.Highlights;
using RaindropServer.Tags;
using RaindropServer.Common;
using Microsoft.Extensions.DependencyInjection;

namespace RaindropServer.Tests;

public class IntegrationTests : TestBase
{
    public IntegrationTests() : base(s =>
    {
        s.AddTransient<CollectionsTools>();
        s.AddTransient<RaindropsTools>();
        s.AddTransient<HighlightsTools>();
        s.AddTransient<TagsTools>();
    })
    { }

    [Fact(Skip = "Requires live Raindrop API")]
    public async Task FullFlow()
    {
        var collections = Provider.GetRequiredService<CollectionsTools>();
        int rootCollectionId = (await collections.CreateCollectionAsync(new Collection { Title = "Integration Root Collection" })).Item.Id;
        int childCollectionId = (await collections.CreateCollectionAsync(new Collection { Title = "Integration Child Collection", Parent = new IdRef { Id = rootCollectionId } })).Item.Id;

        var raindropsTool = Provider.GetRequiredService<RaindropsTools>();
        long firstRaindropId = (await raindropsTool.CreateBookmarkAsync(new RaindropCreateRequest
        {
            CollectionId = rootCollectionId,
            Link = "https://example.com/1",
            Title = "Integration Raindrop One",
            Tags = ["TagOne"],
            Note = "first"
        })).Item.Id;
        long secondRaindropId = (await raindropsTool.CreateBookmarkAsync(new RaindropCreateRequest
        {
            CollectionId = rootCollectionId,
            Link = "https://example.com/2",
            Title = "Integration Raindrop Two",
            Tags = ["TagTwo"],
            Note = "second"
        })).Item.Id;

        var highlights = Provider.GetRequiredService<HighlightsTools>();
        var tags = Provider.GetRequiredService<TagsTools>();
        try
        {
            var highlight = await highlights.CreateHighlightAsync(firstRaindropId, new HighlightCreateRequest { Text = "Integration Highlight", Note = "int" });
            string highlightId = highlight.Item.Highlights.Last().Id!;
            await raindropsTool.UpdateBookmarkAsync(secondRaindropId, new RaindropUpdateRequest { Link = "https://example.com/updated", CollectionId = childCollectionId });
            await tags.RenameTagAsync("TagTwo", "TagTwoRenamed");
            var tagList = await tags.ListTagsAsync();
            Assert.Contains(tagList.Items, t => t.Id == "TagTwoRenamed");
            var childCollections = await collections.ListChildCollectionsAsync();
            Assert.Contains(childCollections.Items, c => c.Id == childCollectionId);
        }
        finally
        {
            await raindropsTool.DeleteBookmarkAsync(firstRaindropId);
            await raindropsTool.DeleteBookmarkAsync(secondRaindropId);
            await collections.DeleteCollectionAsync(childCollectionId);
            await collections.DeleteCollectionAsync(rootCollectionId);
            var finalTags = await tags.ListTagsAsync();
            Assert.DoesNotContain(finalTags.Items, t => t.Id == "TagTwoRenamed");
        }
    }
}
