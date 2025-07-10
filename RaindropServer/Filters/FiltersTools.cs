using System.ComponentModel;
using ModelContextProtocol.Server;
using RaindropServer.Common;

namespace RaindropServer.Filters;

[McpServerToolType]
public class FiltersTools(IFiltersApi api) : RaindropToolBase<IFiltersApi>(api)
{
    [McpServerTool(Destructive = false, Idempotent = true, ReadOnly = true,
        Title = "Get Available Filters"),
     Description("Retrieves available filters for a specific collection or all bookmarks.")]
    public Task<AvailableFilters> GetAvailableFiltersAsync(
        [Description("The ID of the collection to retrieve filters for. Use 0 for all collections.")] long collectionId,
        [Description("Sort tags by 'count' (default) or '_id' (name)." )] string? tagsSort = null,
        [Description("A search query to filter the bookmarks." )] string? search = null)
        => Api.GetAsync(collectionId, tagsSort, search);
}
