using Refit;
using RaindropServer.Common;

namespace RaindropServer.Tags;

public interface ITagsApi
{
    [Get("/tags")]
    Task<ItemsResponse<TagInfo>> ListTagsAsync();

    [Get("/tags/{collectionId}")]
    Task<ItemsResponse<TagInfo>> ListTagsForCollectionAsync(int collectionId);

    [Put("/tags")]
    Task<SuccessResponse> RenameTagAsync([Body] TagBulkUpdateRequest payload);

    [Put("/tags/{collectionId}")]
    Task<SuccessResponse> RenameTagForCollectionAsync(int collectionId, [Body] TagBulkUpdateRequest payload);

    [Delete("/tags")]
    Task<SuccessResponse> DeleteTagsAsync([Body] TagBulkUpdateRequest payload);

    [Delete("/tags/{collectionId}")]
    Task<SuccessResponse> DeleteTagsForCollectionAsync(int collectionId, [Body] TagBulkUpdateRequest payload);
}
