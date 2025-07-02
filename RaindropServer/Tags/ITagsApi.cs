using Refit;

namespace RaindropTools.Tags;

public interface ITagsApi
{
    [Get("/tags")]
    Task<string> ListTags();

    [Put("/tags")] 
    Task<string> RenameTag([Body] object payload);

    [Put("/tags/{collectionId}")]
    Task<string> RenameTagForCollection(int collectionId, [Body] object payload);

    [Delete("/tag/{tag}")]
    Task<string> DeleteTag(string tag);
}
