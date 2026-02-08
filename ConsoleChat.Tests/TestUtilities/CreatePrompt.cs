using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using NSubstitute;
using SemanticKernelChat.Infrastructure;

namespace ConsoleChat.Tests.TestUtilities;

internal static class PromptFactory
{
    public static McpPromptCollection CreateCollectionWithPrompt(string name)
    {
        var prompt = new Prompt { Name = name, Description = string.Empty, Arguments = [] };
        var clientPrompt = new McpClientPrompt(Substitute.For<McpClient>(), prompt);
        var entry = new McpServerState.ServerEntry
        {
            Enabled = true,
            Status = ServerStatus.Ready,
            Prompts = new[] { clientPrompt }
        };
        var dict = new ConcurrentDictionary<string, McpServerState.ServerEntry>(StringComparer.OrdinalIgnoreCase)
        {
            ["server"] = entry
        };
        var state = new McpServerState(dict);
        var manager = new McpServerManager(state);
        return new McpPromptCollection(manager);
    }

    public static McpPromptCollection CreateCollectionWithPrompts(params string[] names)
    {
        var entry = new McpServerState.ServerEntry
        {
            Enabled = true,
            Status = ServerStatus.Ready
        };

        entry.Prompts = names
            .Select(name => new McpClientPrompt(
                Substitute.For<McpClient>(),
                new Prompt { Name = name, Description = string.Empty, Arguments = [] }))
            .ToList();

        var dict = new ConcurrentDictionary<string, McpServerState.ServerEntry>(StringComparer.OrdinalIgnoreCase)
        {
            ["server"] = entry
        };
        var state = new McpServerState(dict);
        var manager = new McpServerManager(state);
        return new McpPromptCollection(manager);
    }
}
