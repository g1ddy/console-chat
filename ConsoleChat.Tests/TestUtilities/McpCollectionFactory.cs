using System;
using System.Linq;
using System.Collections.Concurrent;
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
        var dict = new ConcurrentDictionary<string, McpServerState.ServerEntry>(StringComparer.OrdinalIgnoreCase);
        foreach (var s in servers)
        {
            dict[s] = new McpServerState.ServerEntry
            {
                Enabled = true,
                Status = ServerStatus.Ready
            };
        }

        var state = new McpServerState(dict);
        var manager = new McpServerManager(state);
        return new McpToolCollection(manager);
    }
}
