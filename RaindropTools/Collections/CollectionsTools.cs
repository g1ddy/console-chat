using System.ComponentModel;
using ModelContextProtocol.Server;
using RaindropTools.Common;

namespace RaindropTools.Collections;

[McpServerToolType]
public class CollectionsTools
{
    private readonly ICollectionsApi _api;

    public CollectionsTools(ICollectionsApi api)
    {
        _api = api;
    }

    [McpServerTool, Description("List all collections for the current user")]
    public Task<ItemsResponse<Collection>> List() => _api.ListCollections();

    [McpServerTool, Description("Get details for a collection by id")]
    public Task<ItemResponse<Collection>> Get(int id) => _api.GetCollection(id);

    [McpServerTool, Description("Create a new collection")]
    public Task<ItemResponse<Collection>> Create(Collection collection) => _api.CreateCollection(collection);

    [McpServerTool, Description("Update an existing collection")]
    public Task<ItemResponse<Collection>> Update(int id, Collection collection) => _api.UpdateCollection(id, collection);

    [McpServerTool, Description("Delete a collection")]
    public Task<SuccessResponse> Delete(int id) => _api.DeleteCollection(id);

    [McpServerTool, Description("Update order of child collections")]
    public Task<ItemResponse<Collection>> UpdateChildren(int parentId, ChildCollectionsUpdate update) => _api.UpdateChildren(parentId, update);
}
