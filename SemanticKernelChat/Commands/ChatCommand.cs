using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;
using SemanticKernelChat.Console;
using Spectre.Console.Cli;

namespace SemanticKernelChat.Commands;

public sealed class ChatCommand : ChatCommandBase
{
    public ChatCommand(
        IChatClient chatClient,
        IChatHistoryService history,
        IChatController controller,
        IChatConsole console,
        IEnumerable<McpClientTool> tools)
        : base(chatClient, history, controller, console, tools)
    {
    }

    protected override Task SendAndDisplayAsync(
        IChatClient chatClient,
        IChatHistoryService history,
        IReadOnlyList<McpClientTool> tools) =>
        Controller.SendAndDisplayAsync(chatClient, history, tools);
}
