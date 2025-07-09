using RaindropServer.Raindrops;
using Microsoft.Extensions.DependencyInjection;

namespace RaindropServer.Tests;

public class RaindropsTests : TestBase
{
    public RaindropsTests() : base(s => s.AddTransient<RaindropsTools>()) { }

    [Fact(Skip="Requires live Raindrop API")]
    public async Task Crud()
    {
        var raindropsTool = Provider.GetRequiredService<RaindropsTools>();
        var createResponse = await raindropsTool.CreateBookmarkAsync(null, "https://example.com", "Raindrops Crud - Create");
        long raindropId = createResponse.Item.Id;
        try
        {
            await raindropsTool.UpdateBookmarkAsync(raindropId, title: "Raindrops Crud - Updated");
            await raindropsTool.UpdateBookmarksAsync(0, new RaindropBulkUpdate { Ids = [ raindropId ], Important = true });
            // add a delay before searching
            await Task.Delay(5000);
            var search = await raindropsTool.ListBookmarksAsync(0, "example");
            Assert.Contains(search.Items, r => r.Id == raindropId);
            var retrieved = await raindropsTool.GetBookmarkAsync(raindropId);
            Assert.Equal("Raindrops Crud - Updated", retrieved.Item.Title);
        }
        finally
        {
            await raindropsTool.DeleteBookmarkAsync(raindropId);
        }
    }
}
