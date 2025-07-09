using System.ComponentModel;
using ModelContextProtocol.Server;
using RaindropServer.Common;

namespace RaindropServer.Collections;

[McpServerToolType]
public class CollectionsTools(ICollectionsApi api) :
    RaindropToolBase<ICollectionsApi>(api)
{
    [McpServerTool, Description("List all collections for the current user")]
    public Task<ItemsResponse<Collection>> ListCollectionsAsync() => Api.ListAsync();

    [McpServerTool, Description("Get details for a collection by id")]
    public Task<ItemResponse<Collection>> GetCollectionAsync(int id) => Api.GetAsync(id);

    [McpServerTool, Description("Create a new collection")]
    public Task<ItemResponse<Collection>> CreateCollectionAsync(Collection collection) => Api.CreateAsync(collection);

    [McpServerTool, Description("Update an existing collection")]
    public Task<ItemResponse<Collection>> UpdateCollectionAsync(int id, Collection collection) => Api.UpdateAsync(id, collection);

    [McpServerTool, Description("Delete a collection")]
    public Task<SuccessResponse> DeleteCollectionAsync(int id) => Api.DeleteAsync(id);

    [McpServerTool, Description("List nested child collections")]
    public Task<ItemsResponse<Collection>> ListChildCollectionsAsync() => Api.ListChildrenAsync();
}
