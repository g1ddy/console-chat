using SemanticKernelChat.Infrastructure;

namespace ConsoleChat.Tests.TestUtilities;

internal static class McpCollectionFactory
{
    public static McpToolCollection CreateToolCollection()
        => new McpToolCollection(new McpServerState());

    public static McpPromptCollection CreatePromptCollection()
        => new McpPromptCollection(new McpServerState());

    public static McpToolCollection CreateToolCollection(params string[] servers)
    {
        var dict = new Dictionary<string, McpServerState.ServerEntry>(StringComparer.OrdinalIgnoreCase);
        foreach (var name in servers)
        {
            dict[name] = new McpServerState.ServerEntry
            {
                Enabled = true,
                Status = ServerStatus.Ready
            };
        }
        var state = new McpServerState(dict);
        return new McpToolCollection(state);
    }
}
