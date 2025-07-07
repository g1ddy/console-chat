using System.ComponentModel;
using ModelContextProtocol.Server;

namespace RaindropServer.Common;

/// <summary>
/// Base class for Raindrop API tools providing the injected API instance and
/// standard CRUD operations.
/// </summary>
public abstract class RaindropToolBase<TApi, TEntity, TKey>
    where TApi : ICommonApi<TEntity, TKey>
{
    protected TApi Api { get; }

    protected RaindropToolBase(TApi api)
    {
        Api = api;
    }

    [McpServerTool, Description("Create a new entity")] 
    public Task<ItemResponse<TEntity>> CreateAsync(TEntity entity) =>
        Api.CreateAsync(entity);

    [McpServerTool, Description("Get an entity by id")]
    public Task<ItemResponse<TEntity>> GetAsync(TKey id) => Api.GetAsync(id);

    [McpServerTool, Description("Update an entity")]
    public Task<ItemResponse<TEntity>> UpdateAsync(TKey id, TEntity entity) =>
        Api.UpdateAsync(id, entity);

    [McpServerTool, Description("Delete an entity")]
    public Task<SuccessResponse> DeleteAsync(TKey id) => Api.DeleteAsync(id);
}
