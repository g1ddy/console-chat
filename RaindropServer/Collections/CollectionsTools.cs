using System.ComponentModel;
using ModelContextProtocol.Server;
using RaindropServer.Common;

namespace RaindropServer.Collections;

[McpServerToolType]
public class CollectionsTools(ICollectionsApi api) :
    RaindropToolBase<ICollectionsApi, Collection, int>(api)
{

    [McpServerTool, Description("List all collections for the current user")]
    public Task<ItemsResponse<Collection>> ListAsync() => Api.ListAsync();


    [McpServerTool, Description("List nested child collections")]
    public Task<ItemsResponse<Collection>> ListChildrenAsync() => Api.ListChildrenAsync();
}
