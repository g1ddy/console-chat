using RaindropTools.Collections;
using RaindropTools.Raindrops;
using RaindropTools.Highlights;
using RaindropTools.Tags;
using RaindropTools.Common;
using Microsoft.Extensions.DependencyInjection;

namespace RaindropTools.Tests;

public class IntegrationTests : TestBase
{
    public IntegrationTests() : base(s =>
    {
        s.AddTransient<CollectionsTools>();
        s.AddTransient<RaindropsTools>();
        s.AddTransient<HighlightsTools>();
        s.AddTransient<TagsTools>();
    }) { }

    [Fact(Skip="Requires live Raindrop API")]
    public async Task FullFlow()
    {
        var collections = Provider.GetRequiredService<CollectionsTools>();
        int rootCollectionId = (await collections.Create(new Collection { Title = "Integration Root Collection" })).Item.Id;
        int childCollectionId = (await collections.Create(new Collection { Title = "Integration Child Collection", Parent = new IdRef { Id = rootCollectionId } })).Item.Id;

        var raindropsTool = Provider.GetRequiredService<RaindropsTools>();
        long firstRaindropId = (await raindropsTool.Create(rootCollectionId, "https://example.com/1", "Integration Raindrop One", tags: ["TagOne"])).Item.Id;
        long secondRaindropId = (await raindropsTool.Create(rootCollectionId, "https://example.com/2", "Integration Raindrop Two", tags: ["TagTwo"])).Item.Id;

        var highlights = Provider.GetRequiredService<HighlightsTools>();
        var tags = Provider.GetRequiredService<TagsTools>();
        try
        {
            var highlight = await highlights.Create(firstRaindropId, "Integration Highlight");
            string highlightId = highlight.Item.Highlights.Last().Id!;
            await raindropsTool.Update(secondRaindropId, link: "https://example.com/updated", collectionId: childCollectionId);
            await tags.Rename("TagTwo", "TagTwoRenamed");
            var tagList = await tags.List();
            Assert.Contains(tagList.Items, t => t == "TagTwoRenamed");
            var childCollections = await collections.ListChildren();
            Assert.Contains(childCollections.Items, c => c.Id == childCollectionId);
        }
        finally
        {
            await raindropsTool.Delete(firstRaindropId);
            await raindropsTool.Delete(secondRaindropId);
            await collections.Delete(childCollectionId);
            await collections.Delete(rootCollectionId);
            var finalTags = await tags.List();
            Assert.DoesNotContain(finalTags.Items, t => t == "TagTwoRenamed");
        }
    }
}
