using Refit;

namespace RaindropTools.Tags;

public interface ITagsApi
{
    [Get("/tags")]
    Task<string> ListTags();

    [Put("/tag/{oldTag}")]
    Task<string> RenameTag(string oldTag, [Body] object payload);

    [Delete("/tag/{tag}")]
    Task<string> DeleteTag(string tag);
}
