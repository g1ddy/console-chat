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
    }) { }

    [Fact(Skip="Requires live Raindrop API")]
    public async Task FullFlow()
    {
        var collections = Provider.GetRequiredService<CollectionsTools>();
        int rootCollectionId = (await collections.CreateAsync(new Collection { Title = "Integration Root Collection" })).Item.Id;
        int childCollectionId = (await collections.CreateAsync(new Collection { Title = "Integration Child Collection", Parent = new IdRef { Id = rootCollectionId } })).Item.Id;

        var raindropsTool = Provider.GetRequiredService<RaindropsTools>();
        long firstRaindropId = (await raindropsTool.CreateAsync(rootCollectionId, "https://example.com/1", "Integration Raindrop One", tags: ["TagOne"])).Item.Id;
        long secondRaindropId = (await raindropsTool.CreateAsync(rootCollectionId, "https://example.com/2", "Integration Raindrop Two", tags: ["TagTwo"])).Item.Id;

        var highlights = Provider.GetRequiredService<HighlightsTools>();
        var tags = Provider.GetRequiredService<TagsTools>();
        try
        {
            var highlight = await highlights.CreateAsync(firstRaindropId, "Integration Highlight");
            string highlightId = highlight.Item.Highlights.Last().Id!;
            await raindropsTool.UpdateAsync(secondRaindropId, link: "https://example.com/updated", collectionId: childCollectionId);
            await tags.RenameAsync("TagTwo", "TagTwoRenamed");
            var tagList = await tags.ListAsync();
            Assert.Contains(tagList.Items, t => t.Id == "TagTwoRenamed");
            var childCollections = await collections.ListChildrenAsync();
            Assert.Contains(childCollections.Items, c => c.Id == childCollectionId);
        }
        finally
        {
            await raindropsTool.DeleteAsync(firstRaindropId);
            await raindropsTool.DeleteAsync(secondRaindropId);
            await collections.DeleteAsync(childCollectionId);
            await collections.DeleteAsync(rootCollectionId);
            var finalTags = await tags.ListAsync();
            Assert.DoesNotContain(finalTags.Items, t => t.Id == "TagTwoRenamed");
        }
    }
}
