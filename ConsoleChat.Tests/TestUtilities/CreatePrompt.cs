using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using SemanticKernelChat.Infrastructure;

namespace ConsoleChat.Tests.TestUtilities;

internal static class PromptFactory
{
    public static McpPromptCollection CreateCollectionWithPrompt(string name)
    {
        var prompt = new Prompt { Name = name, Description = string.Empty, Arguments = new() };
        var ctor = typeof(McpClientPrompt).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { typeof(IMcpClient), typeof(Prompt) }, null)!;
        var clientPrompt = (McpClientPrompt)ctor.Invoke(new object?[] { null, prompt });
        var entry = new McpServerState.ServerEntry
        {
            Enabled = true,
            Status = ServerStatus.Ready
        };
        entry.Prompts.Add(clientPrompt);
        var dict = new ConcurrentDictionary<string, McpServerState.ServerEntry>(StringComparer.OrdinalIgnoreCase)
        {
            ["server"] = entry
        };
        var state = new McpServerState(dict);
        var manager = new McpServerManager(state);
        return new McpPromptCollection(manager);
    }
}
