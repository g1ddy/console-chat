using System.ComponentModel;
using ModelContextProtocol.Server;
using RaindropServer.Common;

namespace RaindropServer.Collections;

[McpServerToolType]
public class CollectionsTools(ICollectionsApi api) :
    RaindropToolBase<ICollectionsApi>(api)
{
    [McpServerTool(Destructive = false, Idempotent = true, ReadOnly = true,
        Title = "List Collections"),
     Description("List all collections for the current user")]
    public Task<ItemsResponse<Collection>> ListCollectionsAsync() => Api.ListAsync();

    [McpServerTool(Destructive = false, Idempotent = true, ReadOnly = true,
        Title = "Get Collection"),
     Description("Get details for a collection by id")]
    public Task<ItemResponse<Collection>> GetCollectionAsync([Description("The ID of the collection to retrieve")] int id)
        => Api.GetAsync(id);

    [McpServerTool(Title = "Create Collection"),
     Description("Create a new collection")]
    public Task<ItemResponse<Collection>> CreateCollectionAsync([Description("The collection details to create")] Collection collection)
        => Api.CreateAsync(collection);

    [McpServerTool(Idempotent = true, Title = "Update Collection"),
     Description("Update an existing collection")]
    public Task<ItemResponse<Collection>> UpdateCollectionAsync(
        [Description("ID of the collection to update")] int id,
        [Description("Updated collection data")] Collection collection)
        => Api.UpdateAsync(id, collection);

    [McpServerTool(Idempotent = true, Title = "Delete Collection"),
     Description("Delete a collection")]
    public Task<SuccessResponse> DeleteCollectionAsync([Description("ID of the collection to delete")] int id)
        => Api.DeleteAsync(id);

    [McpServerTool(Destructive = false, Idempotent = true, ReadOnly = true,
        Title = "List Child Collections"),
     Description("List nested child collections")]
    public Task<ItemsResponse<Collection>> ListChildCollectionsAsync() => Api.ListChildrenAsync();
}
