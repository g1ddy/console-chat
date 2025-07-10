using Refit;
using RaindropServer.Common;

namespace RaindropServer.Filters;

public interface IFiltersApi : ICommonApi<Filter, int>
{
    [Get("/filters")]
    Task<ItemsResponse<Filter>> ListAsync();

    [Get("/filter/{id}")]
    new Task<ItemResponse<Filter>> GetAsync(int id);

    [Post("/filter")]
    new Task<ItemResponse<Filter>> CreateAsync([Body] Filter filter);

    [Put("/filter/{id}")]
    new Task<ItemResponse<Filter>> UpdateAsync(int id, [Body] Filter filter);

    [Delete("/filter/{id}")]
    new Task<SuccessResponse> DeleteAsync(int id);
}
