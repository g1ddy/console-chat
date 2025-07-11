using Refit;

namespace RaindropServer.Filters;

public interface IFiltersApi
{
    [Get("/filters/{collectionId}")]
    Task<AvailableFilters> GetAsync(long collectionId,
        [AliasAs("tagsSort")] string? tagsSort = null,
        [AliasAs("search")] string? search = null);
}
