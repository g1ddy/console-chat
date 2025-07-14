using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using ModelContextProtocol.Server;
using RaindropServer.Common;

namespace RaindropServer.Collections;

[McpServerToolType]
public class CollectionsTools(ICollectionsApi api) :
    RaindropToolBase<ICollectionsApi>(api)
{
    [McpServerTool(Destructive = false, Idempotent = true, ReadOnly = true,
        Title = "List Collections"),
     Description("Retrieves all top-level (root) collections.")]
    public Task<ItemsResponse<Collection>> ListCollectionsAsync() => Api.ListAsync();

    [McpServerTool(Destructive = false, Idempotent = true, ReadOnly = true,
        Title = "Get Collection"),
     Description("Retrieves a single collection by its unique ID.")]
    public Task<ItemResponse<Collection>> GetCollectionAsync([Description("The ID of the collection to retrieve")] int id)
        => Api.GetAsync(id);

    [McpServerTool(Title = "Create Collection"),
     Description("Creates a new collection.")]
    public Task<ItemResponse<Collection>> CreateCollectionAsync([Description("The collection details to create")] Collection collection)
        => Api.CreateAsync(collection);

    [McpServerTool(Idempotent = true, Title = "Update Collection"),
     Description("Updates an existing collection.")]
    public Task<ItemResponse<Collection>> UpdateCollectionAsync(
        [Description("ID of the collection to update")] int id,
        [Description("Updated collection data")] Collection collection)
        => Api.UpdateAsync(id, collection);

    [McpServerTool(Idempotent = true, Title = "Delete Collection"),
     Description("Removes a collection. Bookmarks within it are moved to the Trash.")]
    public Task<SuccessResponse> DeleteCollectionAsync([Description("ID of the collection to delete")] int id)
        => Api.DeleteAsync(id);

    [McpServerTool(Destructive = false, Idempotent = true, ReadOnly = true,
        Title = "List Child Collections"),
     Description("Retrieves all nested (child) collections.")]
    public Task<ItemsResponse<Collection>> ListChildCollectionsAsync() => Api.ListChildrenAsync();

    [McpServerTool(Idempotent = true, Title = "Merge Collections"),
     Description("Merge multiple collections into a destination collection.")]
    public Task<SuccessResponse> MergeCollectionsAsync(
        [Description("Collection ID where listed collection ids will be merged")] int to,
        [Description("Collection IDs to merge")] IEnumerable<int> ids)
    {
        var payload = new CollectionsMergeRequest { To = to, Ids = ids.ToList() };
        return Api.MergeAsync(payload);
    }
}
