using RaindropServer.Collections;
using RaindropServer.Raindrops;
using RaindropServer.Highlights;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

namespace RaindropServer.Tests;

public class HighlightsTests : TestBase
{
    public HighlightsTests() : base(s =>
    {
        s.AddTransient<CollectionsTools>();
        s.AddTransient<RaindropsTools>();
        s.AddTransient<HighlightsTools>();
    }) { }

    [Fact(Skip="Requires live Raindrop API")]
    public async Task Crud()
    {
        var collections = Provider.GetRequiredService<CollectionsTools>();
        int collectionId = (await collections.CreateCollectionAsync(new Collection { Title = "Highlights Crud - Collection" })).Item.Id;
        var raindropService = Provider.GetRequiredService<RaindropsTools>();
        long raindropId = (await raindropService.CreateBookmarkAsync(collectionId, "https://example.com/hl", "Highlights Crud - Raindrop")).Item.Id;
        var highlights = Provider.GetRequiredService<HighlightsTools>();
        try
        {
            var newHighlight = await highlights.CreateHighlightAsync(raindropId, "Highlights Crud - New");
            string highlightId = newHighlight.Item.Highlights.Last().Id!;
            await highlights.UpdateHighlightAsync(raindropId, highlightId, text: "Highlights Crud - Updated");
            var listAll = await highlights.ListHighlightsAsync();
            Assert.True(listAll.Items.Count > 0);
            var listByCollection = await highlights.ListHighlightsByCollectionAsync(collectionId);
            Assert.Contains(listByCollection.Items, h => h.Id == highlightId);
            var retrieved = await highlights.GetBookmarkHighlightsAsync(raindropId);
            Assert.Contains(retrieved.Item.Highlights, h => h.Id == highlightId);
            await highlights.DeleteHighlightAsync(raindropId, highlightId);
        }
        finally
        {
            await raindropService.DeleteBookmarkAsync(raindropId);
            await collections.DeleteCollectionAsync(collectionId);
        }
    }
}
