using RaindropTools.Collections;
using Microsoft.Extensions.DependencyInjection;

namespace RaindropTools.Tests;

public class CollectionsTests : TestBase
{
    public CollectionsTests() : base(s => s.AddTransient<CollectionsTools>()) { }

    [Fact(Skip="Requires live Raindrop API")]
    public async Task Crud()
    {
        var tools = Provider.GetRequiredService<CollectionsTools>();
        var create = await tools.Create(new CollectionUpdate { Title = "test" });
        int id = create.Item.Id;
        try
        {
            await tools.Update(id, new CollectionUpdate { Title = "updated" });
            await tools.List();
            var get = await tools.Get(id);
            Assert.Equal("updated", get.Item.Title);
        }
        finally
        {
            await tools.Delete(id);
        }
    }

    [Fact(Skip="Requires live Raindrop API")]
    public async Task UpdateChildren()
    {
        var tools = Provider.GetRequiredService<CollectionsTools>();
        int parent = (await tools.Create(new CollectionUpdate { Title = "parent" })).Item.Id;
        int child = (await tools.Create(new CollectionUpdate { Title = "child", ParentId = parent })).Item.Id;
        try
        {
            await tools.UpdateChildren(parent, new ChildCollectionsUpdate { Children = [ child ] });
        }
        finally
        {
            await tools.Delete(child);
            await tools.Delete(parent);
        }
    }
}
