using RaindropServer.Collections;
using RaindropServer.Common;
using Microsoft.Extensions.DependencyInjection;

namespace RaindropServer.Tests;

public class CollectionsTests : TestBase
{
    public CollectionsTests() : base(s => s.AddTransient<CollectionsTools>()) { }

    [Fact(Skip="Requires live Raindrop API")]
    public async Task Crud()
    {
        var collections = Provider.GetRequiredService<CollectionsTools>();
        var createResponse = await collections.CreateAsync(new Collection { Title = "Collections Crud - Create" });
        int collectionId = createResponse.Item.Id;
        try
        {
            await collections.UpdateAsync(collectionId, new Collection { Title = "Collections Crud - Updated" });
            var list = await collections.ListAsync();
            Assert.Contains(list.Items, c => c.Id == collectionId);
            var retrieved = await collections.GetAsync(collectionId);
            Assert.Equal("Collections Crud - Updated", retrieved.Item.Title);
        }
        finally
        {
            await collections.DeleteAsync(collectionId);
        }
    }

    [Fact(Skip="Requires live Raindrop API")]
    public async Task ListChildren()
    {
        var collections = Provider.GetRequiredService<CollectionsTools>();
        int parentCollectionId = (await collections.CreateAsync(new Collection { Title = "Collections ListChildren - Parent" })).Item.Id;
        int childCollectionId = (await collections.CreateAsync(new Collection { Title = "Collections ListChildren - Child", Parent = new IdRef { Id = parentCollectionId } })).Item.Id;
        try
        {
            var result = await collections.ListChildrenAsync();
            Assert.Contains(result.Items, c => c.Id == childCollectionId);
        }
        finally
        {
            await collections.DeleteAsync(childCollectionId);
            await collections.DeleteAsync(parentCollectionId);
            var finalList = await collections.ListAsync();
            Assert.DoesNotContain(finalList.Items, c => c.Id == parentCollectionId);
        }
    }
}
