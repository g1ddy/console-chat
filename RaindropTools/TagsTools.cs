using System.ComponentModel;
using Refit;
using ModelContextProtocol.Server;

namespace RaindropTools;

[McpServerToolType]
public class TagsTools
{
    private readonly IRaindropApi _api;

    public TagsTools(IRaindropApi api)
    {
        _api = api;
    }

    [McpServerTool, Description("List all tags")]
    public async Task<string> List()
    {
        return await _api.ListTags();
    }

    [McpServerTool, Description("Rename a tag")]
    public async Task<string> Rename(string oldTag, string newTag)
    {
        var payload = new { newName = newTag };
        return await _api.RenameTag(Uri.EscapeDataString(oldTag), payload);
    }

    [McpServerTool, Description("Delete a tag")]
    public async Task<string> Delete(string tag)
    {
        return await _api.DeleteTag(Uri.EscapeDataString(tag));
    }
}
