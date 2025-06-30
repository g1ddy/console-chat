using Refit;

namespace RaindropTools;

/// <summary>
/// Refit interface for the Raindrop API endpoints.
/// </summary>
public interface IRaindropApi
{
    // Collections
    [Get("collections")] Task<ItemsResponse<Collection>> ListCollections();
    [Get("collection/{id}")] Task<ItemResponse<Collection>> GetCollection(int id);
    [Post("collection")] Task<ItemResponse<Collection>> CreateCollection([Body] CollectionUpdate collection);
    [Put("collection/{id}")] Task<ItemResponse<Collection>> UpdateCollection(int id, [Body] CollectionUpdate collection);
    [Delete("collection/{id}")] Task<SuccessResponse> DeleteCollection(int id);
    [Put("collection/{parentId}/children")] Task<ItemResponse<Collection>> UpdateChildCollections(int parentId, [Body] ChildCollectionsUpdate update);

    // Raindrops (bookmarks)
    [Post("raindrop")] Task<ItemResponse<Raindrop>> CreateRaindrop([Body] object payload);
    [Get("raindrop/{id}")] Task<ItemResponse<Raindrop>> GetRaindrop(long id);
    [Put("raindrop/{id}")] Task<ItemResponse<Raindrop>> UpdateRaindrop(long id, [Body] object payload);
    [Delete("raindrop/{id}")] Task<SuccessResponse> DeleteRaindrop(long id);
    [Get("raindrops/{collectionId}")] Task<ItemsResponse<Raindrop>> SearchRaindrops(int collectionId, [AliasAs("search")] string query);

    // Highlights
    [Get("raindrop/{raindropId}/highlights")] Task<string> GetHighlights(long raindropId);
    [Post("raindrop/{raindropId}/highlights")] Task<string> CreateHighlight(long raindropId, [Body] object payload);
    [Put("highlight/{highlightId}")] Task<string> UpdateHighlight(long highlightId, [Body] object payload);
    [Delete("highlight/{highlightId}")] Task<string> DeleteHighlight(long highlightId);

    // Tags
    [Get("tags")] Task<string> ListTags();
    [Put("tag/{tag}")] Task<string> RenameTag(string tag, [Body] object payload);
    [Delete("tag/{tag}")] Task<string> DeleteTag(string tag);

    // User
    [Get("user")] Task<string> GetUser();
    [Put("user")] Task<string> UpdateUser([Body] object payload);
}
