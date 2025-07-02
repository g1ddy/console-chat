using RaindropTools.Collections;
using RaindropTools.Raindrops;
using RaindropTools.Highlights;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

namespace RaindropTools.Tests;

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
        int collectionId = (await collections.Create(new Collection { Title = "Highlights Crud - Collection" })).Item.Id;
        var raindropService = Provider.GetRequiredService<RaindropsTools>();
        long raindropId = (await raindropService.Create(collectionId, "https://example.com/hl", "Highlights Crud - Raindrop")).Item.Id;
        var highlights = Provider.GetRequiredService<HighlightsTools>();
        try
        {
            var newHighlight = await highlights.Create(raindropId, "Highlights Crud - New");
            string highlightId = newHighlight.Item.Highlights.Last().Id!;
            await highlights.Update(raindropId, highlightId, text: "Highlights Crud - Updated");
            var listAll = await highlights.List();
            Assert.True(listAll.Items.Count > 0);
            var listByCollection = await highlights.ListByCollection(collectionId);
            Assert.Contains(listByCollection.Items, h => h.Id == highlightId);
            var retrieved = await highlights.Get(raindropId);
            Assert.Contains(retrieved.Item.Highlights, h => h.Id == highlightId);
            await highlights.Delete(raindropId, highlightId);
        }
        finally
        {
            await raindropService.Delete(raindropId);
            await collections.Delete(collectionId);
        }
    }
}
