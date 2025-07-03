using Refit;
using RaindropTools.Common;

namespace RaindropTools.Highlights;

public interface IHighlightsApi
{
    [Get("/highlights")]
    Task<ItemsResponse<Highlight>> ListHighlights(int? page = null, int? perPage = null);

    [Get("/highlights/{collectionId}")]
    Task<ItemsResponse<Highlight>> ListHighlightsByCollection(int collectionId, int? page = null, int? perPage = null);

    [Get("/raindrop/{id}")]
    Task<ItemResponse<RaindropHighlights>> GetHighlights(long id);

    [Put("/raindrop/{id}")]
    Task<ItemResponse<RaindropHighlights>> UpdateHighlights(long id, [Body] HighlightsBulkUpdate payload);
}
