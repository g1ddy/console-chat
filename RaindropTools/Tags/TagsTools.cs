using System.ComponentModel;
using ModelContextProtocol.Server;

namespace RaindropTools.Tags;

[McpServerToolType]
public class TagsTools
{
    private readonly IRaindropApi _api;

    public TagsTools(IRaindropApi api)
    {
        _api = api;
    }

    [McpServerTool, Description("List all tags")]
    public Task<string> List() => _api.ListTags();

    [McpServerTool, Description("Rename a tag")]
    public Task<string> Rename(string oldTag, string newTag)
    {
        var payload = new { newName = newTag };
        return _api.RenameTag(Uri.EscapeDataString(oldTag), payload);
    }

    [McpServerTool, Description("Delete a tag")]
    public Task<string> Delete(string tag) => _api.DeleteTag(Uri.EscapeDataString(tag));
}
