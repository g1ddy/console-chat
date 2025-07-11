using System;
using System.Linq;
using SemanticKernelChat.Infrastructure;

namespace ConsoleChat.Tests.TestUtilities;

internal static class McpCollectionFactory
{
    public static McpToolCollection CreateToolCollection()
        => new McpToolCollection(new McpServerManager(new McpServerState()));

    public static McpPromptCollection CreatePromptCollection()
        => new McpPromptCollection(new McpServerManager(new McpServerState()));

    public static McpToolCollection CreateToolCollection(params string[] servers)
    {
        var dict = servers.ToDictionary(
            s => s,
            _ => new McpServerState.ServerEntry
            {
                Enabled = true,
                Status = ServerStatus.Ready
            },
            StringComparer.OrdinalIgnoreCase);

        var state = new McpServerState(dict);
        var manager = new McpServerManager(state);
        return new McpToolCollection(manager);
    }
}
