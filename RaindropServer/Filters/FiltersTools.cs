using System.ComponentModel;
using ModelContextProtocol.Server;
using RaindropServer.Common;

namespace RaindropServer.Filters;

[McpServerToolType]
public class FiltersTools(IFiltersApi api) : RaindropToolBase<IFiltersApi>(api)
{
    [McpServerTool(Destructive = false, Idempotent = true, ReadOnly = true,
        Title = "List Filters"),
     Description("List all saved filters")]
    public Task<ItemsResponse<Filter>> ListFiltersAsync() => Api.ListAsync();

    [McpServerTool(Destructive = false, Idempotent = true, ReadOnly = true,
        Title = "Get Filter"),
     Description("Get details for a filter by id")]
    public Task<ItemResponse<Filter>> GetFilterAsync([Description("Filter identifier")] int id)
        => Api.GetAsync(id);

    [McpServerTool(Title = "Create Filter"),
     Description("Create a new filter")]
    public Task<ItemResponse<Filter>> CreateFilterAsync([Description("Filter details")] Filter filter)
        => Api.CreateAsync(filter);

    [McpServerTool(Idempotent = true, Title = "Update Filter"),
     Description("Update an existing filter")]
    public Task<ItemResponse<Filter>> UpdateFilterAsync(
        [Description("Identifier of the filter to update")] int id,
        [Description("Updated filter data")] Filter filter)
        => Api.UpdateAsync(id, filter);

    [McpServerTool(Idempotent = true, Title = "Delete Filter"),
     Description("Delete a filter")]
    public Task<SuccessResponse> DeleteFilterAsync([Description("Identifier of the filter to delete")] int id)
        => Api.DeleteAsync(id);
}
