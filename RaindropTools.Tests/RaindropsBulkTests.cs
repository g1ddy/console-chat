using Microsoft.Extensions.DependencyInjection;
using RaindropTools.Raindrops;
using System.Linq;
using System.Collections.Generic;

namespace RaindropTools.Tests;

public class RaindropsBulkTests : TestBase
{
    public RaindropsBulkTests() : base(s => s.AddTransient<RaindropsTools>()) { }

    [Fact(Skip="Requires live Raindrop API")]
    public async Task BulkEndpoints()
    {
        var tool = Provider.GetRequiredService<RaindropsTools>();
        var items = new List<Raindrop>
        {
            new Raindrop { Link = "https://example.com/bulk1", Title = "Bulk Endpoints One" },
            new Raindrop { Link = "https://example.com/bulk2", Title = "Bulk Endpoints Two" }
        };
        var created = await tool.CreateMany(0, items);
        var ids = created.Items.Select(r => r.Id).ToList();
        try
        {
            var list = await tool.List(0, "Bulk Endpoints");
            Assert.True(ids.All(id => list.Items.Any(r => r.Id == id)));

            var update = new RaindropsBulkUpdate
            {
                Ids = ids,
                Important = true,
                Tags = ["bulk-test"]
            };
            await tool.UpdateMany(0, update);

            var updated = await tool.Search(0, "Bulk Endpoints");
            foreach (var id in ids)
            {
                var item = updated.Items.First(r => r.Id == id);
                Assert.True(item.Important);
            }
        }
        finally
        {
            foreach (var id in ids)
                await tool.Delete(id);
        }
    }
}
