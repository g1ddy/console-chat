using System.Collections.Generic;
using RaindropServer.Collections;
using RaindropServer.Common;
using Microsoft.Extensions.DependencyInjection;

namespace RaindropServer.Tests;

public class CollectionsTests : TestBase
{
    public CollectionsTests() : base(s => s.AddTransient<CollectionsTools>()) { }

    [Fact(Skip = "Requires live Raindrop API")]
    public async Task Crud()
    {
        var collections = Provider.GetRequiredService<CollectionsTools>();
        var createResponse = await collections.CreateCollectionAsync(new Collection { Title = "Collections Crud - Create" });
        int collectionId = createResponse.Item.Id;
        try
        {
            await collections.UpdateCollectionAsync(collectionId, new Collection { Title = "Collections Crud - Updated" });
            var list = await collections.ListCollectionsAsync();
            Assert.Contains(list.Items, c => c.Id == collectionId);
            var retrieved = await collections.GetCollectionAsync(collectionId);
            Assert.Equal("Collections Crud - Updated", retrieved.Item.Title);
        }
        finally
        {
            await collections.DeleteCollectionAsync(collectionId);
        }
    }

    [Fact(Skip = "Requires live Raindrop API")]
    public async Task ListChildren()
    {
        var collections = Provider.GetRequiredService<CollectionsTools>();
        int parentCollectionId = (await collections.CreateCollectionAsync(new Collection { Title = "Collections ListChildren - Parent" })).Item.Id;
        int childCollectionId = (await collections.CreateCollectionAsync(new Collection { Title = "Collections ListChildren - Child", Parent = new IdRef { Id = parentCollectionId } })).Item.Id;
        try
        {
            var result = await collections.ListChildCollectionsAsync();
            Assert.Contains(result.Items, c => c.Id == childCollectionId);
        }
        finally
        {
            await collections.DeleteCollectionAsync(childCollectionId);
            await collections.DeleteCollectionAsync(parentCollectionId);
            var finalList = await collections.ListCollectionsAsync();
            Assert.DoesNotContain(finalList.Items, c => c.Id == parentCollectionId);
        }
    }

    [Fact(Skip = "Requires live Raindrop API")]
    public async Task MergeCollections()
    {
        var collections = Provider.GetRequiredService<CollectionsTools>();
        int destinationId = (await collections.CreateCollectionAsync(new Collection { Title = "Collections Merge - Destination" })).Item.Id;
        int sourceId1 = (await collections.CreateCollectionAsync(new Collection { Title = "Collections Merge - Source1" })).Item.Id;
        int sourceId2 = (await collections.CreateCollectionAsync(new Collection { Title = "Collections Merge - Source2" })).Item.Id;

        try
        {
            var result = await collections.MergeCollectionsAsync(destinationId, new List<int> { sourceId1, sourceId2 });
            Assert.True(result.Result);

            var list = await collections.ListCollectionsAsync();
            Assert.DoesNotContain(list.Items, c => c.Id == sourceId1);
            Assert.DoesNotContain(list.Items, c => c.Id == sourceId2);
        }
        finally
        {
            await collections.DeleteCollectionAsync(destinationId);
        }
    }
}
