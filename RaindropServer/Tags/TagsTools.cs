using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using ModelContextProtocol.Server;
using RaindropServer.Common;

namespace RaindropServer.Tags;

[McpServerToolType]
public class TagsTools
{
    private readonly ITagsApi _api;

    public TagsTools(ITagsApi api)
    {
        _api = api;
    }

    [McpServerTool, Description("List all tags or tags for a collection")]
    public Task<ItemsResponse<TagInfo>> ListAsync(int? collectionId = null)
        => collectionId is null
            ? _api.ListTagsAsync()
            : _api.ListTagsForCollectionAsync(collectionId.Value);

    [McpServerTool, Description("Rename a tag")]
    public Task<SuccessResponse> RenameAsync(string oldTag, string newTag, int? collectionId = null)
        => RenameManyAsync([ oldTag ], newTag, collectionId);

    [McpServerTool, Description("Rename multiple tags")]
    public Task<SuccessResponse> RenameManyAsync(IEnumerable<string> tags, string newTag, int? collectionId = null)
    {
        var payload = new TagRenameRequest { Replace = newTag, Tags = tags.ToList() };
        return collectionId is null
            ? _api.RenameTagAsync(payload)
            : _api.RenameTagForCollectionAsync(collectionId.Value, payload);
    }

    [McpServerTool, Description("Delete a tag")]
    public Task<SuccessResponse> DeleteAsync(string tag, int? collectionId = null)
        => DeleteManyAsync([ tag ], collectionId);

    [McpServerTool, Description("Delete multiple tags")]
    public Task<SuccessResponse> DeleteManyAsync(IEnumerable<string> tags, int? collectionId = null)
    {
        var payload = new TagDeleteRequest { Tags = tags.ToList() };
        return collectionId is null
            ? _api.DeleteTagsAsync(payload)
            : _api.DeleteTagsForCollectionAsync(collectionId.Value, payload);
    }
}
