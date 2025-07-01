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

    [Put("/collection/{parentId}/children")]
    Task<ItemResponse<Collection>> UpdateChildren(int parentId, [Body] ChildCollectionsUpdate update);
}
