using System.ComponentModel;
using ModelContextProtocol.Server;
using RaindropServer.Common;

namespace RaindropServer.Collections;

[McpServerToolType]
public class CollectionsTools
{
    private readonly ICollectionsApi _api;

    public CollectionsTools(ICollectionsApi api)
    {
        _api = api;
    }

    [McpServerTool, Description("List all collections for the current user")]
    public Task<ItemsResponse<Collection>> ListAsync() => _api.ListCollectionsAsync();

    [McpServerTool, Description("Get details for a collection by id")]
    public Task<ItemResponse<Collection>> GetAsync(int id) => _api.GetCollectionAsync(id);

    [McpServerTool, Description("Create a new collection")]
    public Task<ItemResponse<Collection>> CreateAsync(Collection collection) => _api.CreateCollectionAsync(collection);

    [McpServerTool, Description("Update an existing collection")]
    public Task<ItemResponse<Collection>> UpdateAsync(int id, Collection collection) => _api.UpdateCollectionAsync(id, collection);

    [McpServerTool, Description("Delete a collection")]
    public Task<SuccessResponse> DeleteAsync(int id) => _api.DeleteCollectionAsync(id);

    [McpServerTool, Description("List nested child collections")]
    public Task<ItemsResponse<Collection>> ListChildrenAsync() => _api.ListChildCollectionsAsync();
}
