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
    Task<SuccessResponse> RenameTagAsync([Body] TagRenameRequest payload);

    [Put("/tags/{collectionId}")]
    Task<SuccessResponse> RenameTagForCollectionAsync(int collectionId, [Body] TagRenameRequest payload);

    [Delete("/tags")]
    Task<SuccessResponse> DeleteTagsAsync([Body] TagDeleteRequest payload);

    [Delete("/tags/{collectionId}")]
    Task<SuccessResponse> DeleteTagsForCollectionAsync(int collectionId, [Body] TagDeleteRequest payload);
}
