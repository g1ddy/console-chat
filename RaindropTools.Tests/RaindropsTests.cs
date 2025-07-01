using RaindropTools.Raindrops;
using Microsoft.Extensions.DependencyInjection;

namespace RaindropTools.Tests;

public class RaindropsTests : TestBase
{
    public RaindropsTests() : base(s => s.AddTransient<RaindropsTools>()) { }

    [Fact(Skip="Requires live Raindrop API")]
    public async Task Crud()
    {
        var tools = Provider.GetRequiredService<RaindropsTools>();
        var create = await tools.Create(0, "https://example.com", "title");
        long id = create.Item.Id;
        try
        {
            await tools.Update(id, title: "upd");
            await tools.Search(0, "example");
            var get = await tools.Get(id);
            Assert.Equal("upd", get.Item.Title);
        }
        finally
        {
            await tools.Delete(id);
        }
    }
}
