using Refit;
using RaindropServer.Common;

namespace RaindropServer.Highlights;

public interface IHighlightsApi
{
    [Get("/highlights")]
    Task<ItemsResponse<Highlight>> ListHighlightsAsync(int? page = null, int? perPage = null);

    [Get("/highlights/{collectionId}")]
    Task<ItemsResponse<Highlight>> ListHighlightsByCollectionAsync(int collectionId, int? page = null, int? perPage = null);

    [Get("/raindrop/{id}")]
    Task<ItemResponse<RaindropHighlights>> GetHighlightsAsync(long id);

    [Put("/raindrop/{id}")]
    Task<ItemResponse<HighlightsBulkUpdateRequest>> UpdateHighlightsAsync(long id, [Body] HighlightsBulkUpdateRequest payload);
}
