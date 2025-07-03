using Refit;
using RaindropTools.Common;

namespace RaindropTools.Raindrops;

public interface IRaindropsApi
{
    [Post("/raindrop")]
    Task<ItemResponse<Raindrop>> CreateRaindrop([Body] Raindrop raindrop);

    [Get("/raindrop/{id}")]
    Task<ItemResponse<Raindrop>> GetRaindrop(long id);

    [Put("/raindrop/{id}")]
    Task<ItemResponse<Raindrop>> UpdateRaindrop(long id, [Body] Raindrop raindrop);

    [Delete("/raindrop/{id}")]
    Task<SuccessResponse> DeleteRaindrop(long id);

    [Get("/raindrops/{collectionId}")]
    Task<ItemsResponse<Raindrop>> GetRaindrops(int collectionId, string? search = null);

    [Post("/raindrops")]
    Task<ItemsResponse<Raindrop>> CreateRaindrops([Body] RaindropsCreateMany payload);

    [Put("/raindrops/{collectionId}")]
    Task<SuccessResponse> UpdateRaindrops(int collectionId, [Body] RaindropsBulkUpdate update,
        bool? nested = null, string? search = null);
}
