using Refit;
using RaindropTools.Common;

namespace RaindropTools.Highlights;

public interface IHighlightsApi
{
    [Get("/highlights")]
    Task<ItemsResponse<Highlight>> ListHighlights([AliasAs("page")] int? page = null, [AliasAs("perpage")] int? perPage = null);

    [Get("/highlights/{collectionId}")]
    Task<ItemsResponse<Highlight>> ListHighlightsByCollection(int collectionId, [AliasAs("page")] int? page = null, [AliasAs("perpage")] int? perPage = null);

    [Get("/raindrop/{id}")]
    Task<ItemResponse<RaindropHighlights>> GetHighlights(long id);

    [Put("/raindrop/{id}")]
    Task<ItemResponse<RaindropHighlights>> UpdateHighlights(long id, [Body] HighlightsBulkUpdate payload);
}
