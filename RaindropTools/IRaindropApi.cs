using Refit;
using RaindropTools.Common;
using RaindropTools.Collections;
using RaindropTools.Raindrops;
using RaindropTools.Highlights;

namespace RaindropTools;

public interface IRaindropApi
{
    // collections
    [Get("/collections")]
    Task<ItemsResponse<Collection>> ListCollections();

    [Get("/collection/{id}")]
    Task<ItemResponse<Collection>> GetCollection(int id);

    [Post("/collection")]
    Task<ItemResponse<Collection>> CreateCollection([Body] CollectionUpdate collection);

    [Put("/collection/{id}")]
    Task<ItemResponse<Collection>> UpdateCollection(int id, [Body] CollectionUpdate collection);

    [Delete("/collection/{id}")]
    Task<SuccessResponse> DeleteCollection(int id);

    [Put("/collection/{parentId}/children")]
    Task<ItemResponse<Collection>> UpdateChildren(int parentId, [Body] ChildCollectionsUpdate update);

    // raindrops
    [Post("/raindrop")]
    Task<ItemResponse<Raindrop>> CreateRaindrop([Body] object payload);

    [Get("/raindrop/{id}")]
    Task<ItemResponse<Raindrop>> GetRaindrop(long id);

    [Put("/raindrop/{id}")]
    Task<ItemResponse<Raindrop>> UpdateRaindrop(long id, [Body] object payload);

    [Delete("/raindrop/{id}")]
    Task<SuccessResponse> DeleteRaindrop(long id);

    [Get("/raindrops/{collectionId}")]
    Task<ItemsResponse<Raindrop>> SearchRaindrops(int collectionId, [AliasAs("search")] string query);

    // highlights
    [Get("/highlights")]
    Task<ItemsResponse<Highlight>> ListHighlights([AliasAs("page")] int? page = null, [AliasAs("perpage")] int? perPage = null);

    [Get("/highlights/{collectionId}")]
    Task<ItemsResponse<Highlight>> ListHighlightsByCollection(int collectionId, [AliasAs("page")] int? page = null, [AliasAs("perpage")] int? perPage = null);

    [Get("/raindrop/{id}")]
    Task<ItemResponse<RaindropHighlights>> GetHighlights(long id);

    [Put("/raindrop/{id}")]
    Task<ItemResponse<RaindropHighlights>> UpdateHighlights(long id, [Body] HighlightsPayload payload);

    // tags
    [Get("/tags")]
    Task<string> ListTags();

    [Put("/tag/{oldTag}")]
    Task<string> RenameTag(string oldTag, [Body] object payload);

    [Delete("/tag/{tag}")]
    Task<string> DeleteTag(string tag);

    // user
    [Get("/user")]
    Task<string> GetUser();

    [Put("/user")]
    Task<string> UpdateUser([Body] object payload);
}
