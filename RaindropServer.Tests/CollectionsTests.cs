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
        var collections = Provider.GetRequiredService<CollectionsTools>();
        var createResponse = await collections.Create(new Collection { Title = "Collections Crud - Create" });
        int collectionId = createResponse.Item.Id;
        try
        {
            await collections.Update(collectionId, new Collection { Title = "Collections Crud - Updated" });
            var list = await collections.List();
            Assert.Contains(list.Items, c => c.Id == collectionId);
            var retrieved = await collections.Get(collectionId);
            Assert.Equal("Collections Crud - Updated", retrieved.Item.Title);
        }
        finally
        {
            await collections.Delete(collectionId);
        }
    }

    [Fact(Skip="Requires live Raindrop API")]
    public async Task ListChildren()
    {
        var collections = Provider.GetRequiredService<CollectionsTools>();
        int parentCollectionId = (await collections.Create(new Collection { Title = "Collections ListChildren - Parent" })).Item.Id;
        int childCollectionId = (await collections.Create(new Collection { Title = "Collections ListChildren - Child", Parent = new IdRef { Id = parentCollectionId } })).Item.Id;
        try
        {
            var result = await collections.ListChildren();
            Assert.Contains(result.Items, c => c.Id == childCollectionId);
        }
        finally
        {
            await collections.Delete(childCollectionId);
            await collections.Delete(parentCollectionId);
            var finalList = await collections.List();
            Assert.DoesNotContain(finalList.Items, c => c.Id == parentCollectionId);
        }
    }
}
