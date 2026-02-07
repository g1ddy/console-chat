using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using NSubstitute;
using SemanticKernelChat.Infrastructure;

namespace ConsoleChat.Tests.TestUtilities;

internal static class PromptFactory
{
    public static McpPromptCollection CreateCollectionWithPrompt(string name)
    {
        var prompt = new Prompt { Name = name, Description = string.Empty, Arguments = new() };
        var ctor = typeof(McpClientPrompt).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { typeof(IMcpClient), typeof(Prompt) }, null)!;
        var clientPrompt = (McpClientPrompt)ctor.Invoke(new object?[] { Substitute.For<IMcpClient>(), prompt });
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
        var ctor = typeof(McpClientPrompt).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { typeof(IMcpClient), typeof(Prompt) }, null)!;

        var prompts = new List<McpClientPrompt>();
        foreach (var name in names)
        {
            var prompt = new Prompt { Name = name, Description = string.Empty, Arguments = new() };
            var clientPrompt = (McpClientPrompt)ctor.Invoke(new object?[] { Substitute.For<IMcpClient>(), prompt });
            prompts.Add(clientPrompt);
        }
        entry.Prompts = prompts;

        var dict = new ConcurrentDictionary<string, McpServerState.ServerEntry>(StringComparer.OrdinalIgnoreCase)
        {
            ["server"] = entry
        };
        var state = new McpServerState(dict);
        var manager = new McpServerManager(state);
        return new McpPromptCollection(manager);
    }
}
