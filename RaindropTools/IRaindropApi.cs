using Refit;

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
    // dynamic payload for create

    [Get("/raindrop/{id}")]
    Task<ItemResponse<Raindrop>> GetRaindrop(long id);

    [Put("/raindrop/{id}")]
    Task<ItemResponse<Raindrop>> UpdateRaindrop(long id, [Body] object payload);

    [Delete("/raindrop/{id}")]
    Task<SuccessResponse> DeleteRaindrop(long id);

    [Get("/raindrops/{collectionId}")]
    Task<ItemsResponse<Raindrop>> SearchRaindrops(int collectionId, [AliasAs("search")] string query);

    // highlights
    [Get("/raindrop/{raindropId}/highlights")]
    Task<string> GetHighlights(long raindropId);

    [Post("/raindrop/{raindropId}/highlights")]
    Task<string> CreateHighlight(long raindropId, [Body] object payload);

    [Put("/highlight/{highlightId}")]
    Task<string> UpdateHighlight(long highlightId, [Body] object payload);

    [Delete("/highlight/{highlightId}")]
    Task<string> DeleteHighlight(long highlightId);

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
