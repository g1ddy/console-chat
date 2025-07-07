using Refit;
using RaindropServer.Common;

namespace RaindropServer.Highlights;

public interface IHighlightsApi
{
    [Get("/highlights")]
    Task<ItemsResponse<Highlight>> ListAsync(int? page = null, int? perPage = null);

    [Get("/highlights/{collectionId}")]
    Task<ItemsResponse<Highlight>> ListByCollectionAsync(int collectionId, int? page = null, int? perPage = null);

    [Get("/raindrop/{id}")]
    Task<ItemResponse<RaindropHighlights>> GetAsync(long id);

    [Put("/raindrop/{id}")]
    Task<ItemResponse<HighlightsBulkUpdateRequest>> UpdateAsync(long id, [Body] HighlightsBulkUpdateRequest payload);
}
