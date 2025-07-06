using System.ComponentModel;
using ModelContextProtocol.Server;
using RaindropTools.Common;

namespace RaindropTools.Tags;

[McpServerToolType]
public class TagsTools
{
    private readonly ITagsApi _api;

    public TagsTools(ITagsApi api)
    {
        _api = api;
    }

    [McpServerTool, Description("List all tags or tags for a collection")]
    public Task<ItemsResponse<TagInfo>> List(int? collectionId = null)
        => collectionId is null
            ? _api.ListTags()
            : _api.ListTagsForCollection(collectionId.Value);

    [McpServerTool, Description("Rename a tag")]
    public Task<SuccessResponse> Rename(string oldTag, string newTag, int? collectionId = null)
    {
        var payload = new { replace = newTag, tags = new[] { oldTag } };
        return collectionId is null
            ? _api.RenameTag(payload)
            : _api.RenameTagForCollection(collectionId.Value, payload);
    }

    [McpServerTool, Description("Delete a tag")]
    public Task<SuccessResponse> Delete(string tag, int? collectionId = null)
    {
        var payload = new { tags = new[] { tag } };
        return collectionId is null
            ? _api.DeleteTags(payload)
            : _api.DeleteTagsForCollection(collectionId.Value, payload);
    }
}
