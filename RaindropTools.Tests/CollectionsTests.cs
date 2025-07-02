using RaindropTools.Collections;
using RaindropTools.Common;
using Microsoft.Extensions.DependencyInjection;

namespace RaindropTools.Tests;

public class CollectionsTests : TestBase
{
    public CollectionsTests() : base(s => s.AddTransient<CollectionsTools>()) { }

    [Fact(Skip="Requires live Raindrop API")]
    public async Task Crud()
    {
        var tools = Provider.GetRequiredService<CollectionsTools>();
        var create = await tools.Create(new Collection { Title = "test" });
        int id = create.Item.Id;
        try
        {
            await tools.Update(id, new Collection { Title = "updated" });
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
    public async Task ListChildren()
    {
        var tools = Provider.GetRequiredService<CollectionsTools>();
        int parent = (await tools.Create(new Collection { Title = "parent" })).Item.Id;
        int child = (await tools.Create(new Collection { Title = "child", Parent = new ParentRef { Id = parent } })).Item.Id;
        try
        {
            var result = await tools.ListChildren();
            Assert.Contains(result.Items, c => c.Id == child);
        }
        finally
        {
            await tools.Delete(child);
            await tools.Delete(parent);
        }
    }
}
