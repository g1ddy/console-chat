using System;
using System.Linq;
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
        var dict = servers.ToDictionary(
            s => s,
            _ => new McpServerState.ServerEntry
            {
                Enabled = true,
                Status = ServerStatus.Ready
            },
            StringComparer.OrdinalIgnoreCase);

        var state = new McpServerState(dict);
        return new McpToolCollection(state);
    }
}
