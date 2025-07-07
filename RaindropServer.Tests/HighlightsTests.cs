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
        int collectionId = (await collections.CreateAsync(new Collection { Title = "Highlights Crud - Collection" })).Item.Id;
        var raindropService = Provider.GetRequiredService<RaindropsTools>();
        long raindropId = (await raindropService.CreateAsync(collectionId, "https://example.com/hl", "Highlights Crud - Raindrop")).Item.Id;
        var highlights = Provider.GetRequiredService<HighlightsTools>();
        try
        {
            var newHighlight = await highlights.CreateAsync(raindropId, "Highlights Crud - New");
            string highlightId = newHighlight.Item.Highlights.Last().Id!;
            await highlights.UpdateAsync(raindropId, highlightId, text: "Highlights Crud - Updated");
            var listAll = await highlights.ListAsync();
            Assert.True(listAll.Items.Count > 0);
            var listByCollection = await highlights.ListByCollectionAsync(collectionId);
            Assert.Contains(listByCollection.Items, h => h.Id == highlightId);
            var retrieved = await highlights.GetAsync(raindropId);
            Assert.Contains(retrieved.Item.Highlights, h => h.Id == highlightId);
            await highlights.DeleteAsync(raindropId, highlightId);
        }
        finally
        {
            await raindropService.DeleteAsync(raindropId);
            await collections.DeleteAsync(collectionId);
        }
    }
}
