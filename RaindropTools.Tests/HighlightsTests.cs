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
        int colId = (await collections.Create(new Collection { Title = "hl" })).Item.Id;
        var drops = Provider.GetRequiredService<RaindropsTools>();
        long dropId = (await drops.Create(colId, "https://example.com/hl", "h")).Item.Id;
        var highlights = Provider.GetRequiredService<HighlightsTools>();
        try
        {
            var create = await highlights.Create(dropId, "test");
            string hId = create.Item.Highlights.Last().Id;
            await highlights.Update(dropId, hId, text: "upd");
            await highlights.List();
            await highlights.ListByCollection(colId);
            var get = await highlights.Get(dropId);
            Assert.Contains(get.Item.Highlights, h => h.Id == hId);
            await highlights.Delete(dropId, hId);
        }
        finally
        {
            await drops.Delete(dropId);
            await collections.Delete(colId);
        }
    }
}
