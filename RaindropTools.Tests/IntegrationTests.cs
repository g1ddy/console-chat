using RaindropTools.Collections;
using RaindropTools.Raindrops;
using RaindropTools.Highlights;
using RaindropTools.Tags;
using Microsoft.Extensions.DependencyInjection;

namespace RaindropTools.Tests;

public class IntegrationTests : TestBase
{
    public IntegrationTests() : base(s =>
    {
        s.AddTransient<CollectionsTools>();
        s.AddTransient<RaindropsTools>();
        s.AddTransient<HighlightsTools>();
        s.AddTransient<TagsTools>();
    }) { }

    [Fact(Skip="Requires live Raindrop API")]
    public async Task FullFlow()
    {
        var collections = Provider.GetRequiredService<CollectionsTools>();
        int rootId = (await collections.Create(new Collection { Title = "root" })).Item.Id;
        int childId = (await collections.Create(new Collection { Title = "child", ParentId = rootId })).Item.Id;

        var bookmarks = Provider.GetRequiredService<RaindropsTools>();
        long b1 = (await bookmarks.Create(rootId, "https://example.com/1", "b1", tags: ["t1"])).Item.Id;
        long b2 = (await bookmarks.Create(rootId, "https://example.com/2", "b2", tags: ["t2"])).Item.Id;

        var highlights = Provider.GetRequiredService<HighlightsTools>();
        var tags = Provider.GetRequiredService<TagsTools>();
        try
        {
            await highlights.Create(b1, "hl");
            await bookmarks.Update(b2, link: "https://example.com/updated", collectionId: childId);
            await tags.Rename("t2", "t22");
            await tags.List();
            await collections.UpdateChildren(rootId, new ChildCollectionsUpdate { Children = [ childId ] });
        }
        finally
        {
            await bookmarks.Delete(b1);
            await bookmarks.Delete(b2);
            await collections.Delete(childId);
            await collections.Delete(rootId);
        }
    }
}
