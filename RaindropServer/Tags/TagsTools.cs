using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using ModelContextProtocol.Server;
using RaindropServer.Common;

namespace RaindropServer.Tags;

[McpServerToolType]
public class TagsTools(ITagsApi api) : RaindropToolBase<ITagsApi>(api)
{


    [McpServerTool(Destructive = false, Idempotent = true, ReadOnly = true,
        Title = "List Tags"),
     Description("List all tags or tags for a collection")]
    public Task<ItemsResponse<TagInfo>> ListTagsAsync([Description("Collection ID or null for all")] int? collectionId = null)
        => collectionId is null
            ? Api.ListAsync()
            : Api.ListForCollectionAsync(collectionId.Value);

    [McpServerTool(Idempotent = true, Title = "Rename Tag"),
     Description("Rename a tag")]
    public Task<SuccessResponse> RenameTagAsync(
        [Description("Existing tag name")] string oldTag,
        [Description("New tag name")] string newTag,
        [Description("Collection ID if scoped")] int? collectionId = null)
        => RenameTagsAsync([ oldTag ], newTag, collectionId);

    [McpServerTool(Idempotent = true, Title = "Rename Tags"),
     Description("Rename multiple tags")]
    public Task<SuccessResponse> RenameTagsAsync(
        [Description("Tags to rename")] IEnumerable<string> tags,
        [Description("Replacement tag name")] string newTag,
        [Description("Collection ID if scoped")] int? collectionId = null)
    {
        var payload = new TagRenameRequest { Replace = newTag, Tags = tags.ToList() };
        return collectionId is null
            ? Api.UpdateAsync(payload)
            : Api.UpdateForCollectionAsync(collectionId.Value, payload);
    }

    [McpServerTool(Idempotent = true, Title = "Delete Tag"),
     Description("Delete a tag")]
    public Task<SuccessResponse> DeleteTagAsync(
        [Description("Tag to delete")] string tag,
        [Description("Collection ID if scoped")] int? collectionId = null)
        => DeleteTagsAsync([ tag ], collectionId);

    [McpServerTool(Idempotent = true, Title = "Delete Tags"),
     Description("Delete multiple tags")]
    public Task<SuccessResponse> DeleteTagsAsync(
        [Description("Tags to delete")] IEnumerable<string> tags,
        [Description("Collection ID if scoped")] int? collectionId = null)
    {
        var payload = new TagDeleteRequest { Tags = tags.ToList() };
        return collectionId is null
            ? Api.DeleteAsync(payload)
            : Api.DeleteForCollectionAsync(collectionId.Value, payload);
    }
}
