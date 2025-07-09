using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using ModelContextProtocol.Server;
using RaindropServer.Common;

namespace RaindropServer.Tags;

[McpServerToolType]
public class TagsTools(ITagsApi api) : RaindropToolBase<ITagsApi>(api)
{


    [McpServerTool, Description("List all tags or tags for a collection")]
    public Task<ItemsResponse<TagInfo>> ListTagsAsync(int? collectionId = null)
        => collectionId is null
            ? Api.ListAsync()
            : Api.ListForCollectionAsync(collectionId.Value);

    [McpServerTool, Description("Rename a tag")]
    public Task<SuccessResponse> RenameTagAsync(string oldTag, string newTag, int? collectionId = null)
        => RenameTagsAsync([ oldTag ], newTag, collectionId);

    [McpServerTool, Description("Rename multiple tags")]
    public Task<SuccessResponse> RenameTagsAsync(IEnumerable<string> tags, string newTag, int? collectionId = null)
    {
        var payload = new TagRenameRequest { Replace = newTag, Tags = tags.ToList() };
        return collectionId is null
            ? Api.UpdateAsync(payload)
            : Api.UpdateForCollectionAsync(collectionId.Value, payload);
    }

    [McpServerTool, Description("Delete a tag")]
    public Task<SuccessResponse> DeleteTagAsync(string tag, int? collectionId = null)
        => DeleteTagsAsync([ tag ], collectionId);

    [McpServerTool, Description("Delete multiple tags")]
    public Task<SuccessResponse> DeleteTagsAsync(IEnumerable<string> tags, int? collectionId = null)
    {
        var payload = new TagDeleteRequest { Tags = tags.ToList() };
        return collectionId is null
            ? Api.DeleteAsync(payload)
            : Api.DeleteForCollectionAsync(collectionId.Value, payload);
    }
}
