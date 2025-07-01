using RaindropTools.Raindrops;
using RaindropTools.Tags;
using Microsoft.Extensions.DependencyInjection;

namespace RaindropTools.Tests;

public class TagsTests : TestBase
{
    public TagsTests() : base(s =>
    {
        s.AddTransient<RaindropsTools>();
        s.AddTransient<TagsTools>();
    }) { }

    [Fact(Skip="Requires live Raindrop API")]
    public async Task Crud()
    {
        var drops = Provider.GetRequiredService<RaindropsTools>();
        var create = await drops.Create(0, "https://example.com/tag", "t", tags: [ "one" ]);
        long id = create.Item.Id;
        var tags = Provider.GetRequiredService<TagsTools>();
        try
        {
            await tags.Rename("one", "two");
            await tags.List();
        }
        finally
        {
            await tags.Delete("two");
            await drops.Delete(id);
        }
    }
}
