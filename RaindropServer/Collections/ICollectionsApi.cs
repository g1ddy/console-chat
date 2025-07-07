using Refit;
using RaindropServer.Common;

namespace RaindropServer.Collections;

public interface ICollectionsApi
{
    [Get("/collections")]
    Task<ItemsResponse<Collection>> ListCollectionsAsync();

    [Get("/collection/{id}")]
    Task<ItemResponse<Collection>> GetCollectionAsync(int id);

    [Post("/collection")]
    Task<ItemResponse<Collection>> CreateCollectionAsync([Body] Collection collection);

    [Put("/collection/{id}")]
    Task<ItemResponse<Collection>> UpdateCollectionAsync(int id, [Body] Collection collection);

    [Delete("/collection/{id}")]
    Task<SuccessResponse> DeleteCollectionAsync(int id);

    [Get("/collections/childrens")]
    Task<ItemsResponse<Collection>> ListChildCollectionsAsync();
}
