using System.ComponentModel;
using ModelContextProtocol.Server;
using RaindropServer.Common;

namespace RaindropServer.Collections;

[McpServerToolType]
public class CollectionsTools(ICollectionsApi api) :
    RaindropToolBase<ICollectionsApi>(api)
{
    [McpServerTool, Description("List all collections for the current user")]
    public Task<ItemsResponse<Collection>> ListAsync() => Api.ListAsync();

    [McpServerTool, Description("Get details for a collection by id")]
    public Task<ItemResponse<Collection>> GetAsync(int id) => Api.GetAsync(id);

    [McpServerTool, Description("Create a new collection")]
    public Task<ItemResponse<Collection>> CreateAsync(Collection collection) => Api.CreateAsync(collection);

    [McpServerTool, Description("Update an existing collection")]
    public Task<ItemResponse<Collection>> UpdateAsync(int id, Collection collection) => Api.UpdateAsync(id, collection);

    [McpServerTool, Description("Delete a collection")]
    public Task<SuccessResponse> DeleteAsync(int id) => Api.DeleteAsync(id);

    [McpServerTool, Description("List nested child collections")]
    public Task<ItemsResponse<Collection>> ListChildrenAsync() => Api.ListChildrenAsync();
}
