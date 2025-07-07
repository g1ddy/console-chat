using Microsoft.Extensions.DependencyInjection;
using RaindropServer.Raindrops;
using RaindropServer.Tags;
using System.Linq;
using System.Collections.Generic;

namespace RaindropServer.Tests;

public class TagsBulkTests : TestBase
{
    public TagsBulkTests() : base(s =>
    {
        s.AddTransient<RaindropsTools>();
        s.AddTransient<TagsTools>();
    }) { }

    [Fact(Skip="Requires live Raindrop API")]
    public async Task BulkEndpoints()
    {
        var raindrops = Provider.GetRequiredService<RaindropsTools>();
        var tags = Provider.GetRequiredService<TagsTools>();
        var items = new List<Raindrop>
        {
            new Raindrop { Link = "https://example.com/tags-bulk1", Title = "Tags Bulk One", Tags = ["TagBulkOne"] },
            new Raindrop { Link = "https://example.com/tags-bulk2", Title = "Tags Bulk Two", Tags = ["TagBulkTwo"] }
        };
        var created = await raindrops.CreateManyAsync(0, items);
        var ids = created.Items.Select(r => r.Id).ToList();
        try
        {
            // allow indexing before listing
            await Task.Delay(5000);
            await tags.RenameManyAsync(["TagBulkOne", "TagBulkTwo"], "TagBulkRenamed");

            await Task.Delay(5000);
            var list = await tags.ListAsync();
            Assert.Contains(list.Items, t => t.Id == "TagBulkRenamed");

            await tags.DeleteManyAsync(["TagBulkRenamed"]);
            await Task.Delay(5000);
            var finalList = await tags.ListAsync();
            Assert.DoesNotContain(finalList.Items, t => t.Id == "TagBulkRenamed");
        }
        finally
        {
            foreach (var id in ids)
                await raindrops.DeleteAsync(id);
        }
    }
}
