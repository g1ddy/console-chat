using Refit;
using RaindropTools.Common;

namespace RaindropTools.Tags;

public interface ITagsApi
{
    [Get("/tags")]
    Task<ItemsResponse<TagInfo>> ListTags();

    [Get("/tags/{collectionId}")]
    Task<ItemsResponse<TagInfo>> ListTagsForCollection(int collectionId);

    [Put("/tags")]
    Task<SuccessResponse> RenameTag([Body] object payload);

    [Put("/tags/{collectionId}")]
    Task<SuccessResponse> RenameTagForCollection(int collectionId, [Body] object payload);

    [Delete("/tags")]
    Task<SuccessResponse> DeleteTags([Body] object payload);

    [Delete("/tags/{collectionId}")]
    Task<SuccessResponse> DeleteTagsForCollection(int collectionId, [Body] object payload);
}
