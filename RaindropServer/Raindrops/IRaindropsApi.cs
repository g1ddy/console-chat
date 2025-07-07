using Refit;
using RaindropServer.Common;

namespace RaindropServer.Raindrops;

public interface IRaindropsApi
{
    [Post("/raindrop")]
    Task<ItemResponse<Raindrop>> CreateRaindropAsync([Body] Raindrop raindrop);

    [Get("/raindrop/{id}")]
    Task<ItemResponse<Raindrop>> GetRaindropAsync(long id);

    [Put("/raindrop/{id}")]
    Task<ItemResponse<Raindrop>> UpdateRaindropAsync(long id, [Body] Raindrop raindrop);

    [Delete("/raindrop/{id}")]
    Task<SuccessResponse> DeleteRaindropAsync(long id);

    [Get("/raindrops/{collectionId}")]
    Task<ItemsResponse<Raindrop>> GetRaindropsAsync(int collectionId, string? search = null);

    [Post("/raindrops")]
    Task<ItemsResponse<Raindrop>> CreateRaindropsAsync([Body] RaindropsCreateMany payload);

    [Put("/raindrops/{collectionId}")]
    Task<SuccessResponse> UpdateRaindropsAsync(int collectionId, [Body] RaindropsBulkUpdate update,
        bool? nested = null, string? search = null);
}
