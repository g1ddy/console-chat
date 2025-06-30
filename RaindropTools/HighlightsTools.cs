using System.ComponentModel;
using Refit;
using ModelContextProtocol.Server;

namespace RaindropTools;

[McpServerToolType]
public class HighlightsTools
{
    private readonly IRaindropApi _api;

    public HighlightsTools(IRaindropApi api)
    {
        _api = api;
    }

    [McpServerTool, Description("Get highlights for a bookmark")]
    public async Task<string> Get(long raindropId)
    {
        return await _api.GetHighlights(raindropId);
    }

    [McpServerTool, Description("Create a highlight for a bookmark")]
    public async Task<string> Create(long raindropId, string text)
    {
        var payload = new { text };
        return await _api.CreateHighlight(raindropId, payload);
    }

    [McpServerTool, Description("Update an existing highlight")]
    public async Task<string> Update(long highlightId, string text)
    {
        var payload = new { text };
        return await _api.UpdateHighlight(highlightId, payload);
    }

    [McpServerTool, Description("Delete a highlight by id")]
    public async Task<string> Delete(long highlightId)
    {
        return await _api.DeleteHighlight(highlightId);
    }
}
