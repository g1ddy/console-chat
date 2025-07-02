using RaindropTools.Raindrops;
using Microsoft.Extensions.DependencyInjection;

namespace RaindropTools.Tests;

public class RaindropsTests : TestBase
{
    public RaindropsTests() : base(s => s.AddTransient<RaindropsTools>()) { }

    [Fact(Skip="Requires live Raindrop API")]
    public async Task Crud()
    {
        var raindropsTool = Provider.GetRequiredService<RaindropsTools>();
        var createResponse = await raindropsTool.Create(null, "https://example.com", "Raindrops Crud - Create");
        long raindropId = createResponse.Item.Id;
        try
        {
            await raindropsTool.Update(raindropId, title: "Raindrops Crud - Updated");
            await raindropsTool.UpdateMany(0, new RaindropsBulkUpdate { Ids = [ raindropId ], Important = true });
            var search = await raindropsTool.Search(0, "example");
            Assert.Contains(search.Items, r => r.Id == raindropId);
            var retrieved = await raindropsTool.Get(raindropId);
            Assert.Equal("Raindrops Crud - Updated", retrieved.Item.Title);
        }
        finally
        {
            await raindropsTool.Delete(raindropId);
        }
    }
}
