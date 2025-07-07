using Refit;
using RaindropTools.Common;

namespace RaindropTools.Collections;

public interface ICollectionsApi
{
    [Get("/collections")]
    Task<ItemsResponse<Collection>> ListCollections();

    [Get("/collection/{id}")]
    Task<ItemResponse<Collection>> GetCollection(int id);

    [Post("/collection")]
    Task<ItemResponse<Collection>> CreateCollection([Body] Collection collection);

    [Put("/collection/{id}")]
    Task<ItemResponse<Collection>> UpdateCollection(int id, [Body] Collection collection);

    [Delete("/collection/{id}")]
    Task<SuccessResponse> DeleteCollection(int id);

    [Get("/collections/childrens")]
    Task<ItemsResponse<Collection>> ListChildCollections();
}
