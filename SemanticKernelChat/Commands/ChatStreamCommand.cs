using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;
using SemanticKernelChat.Console;
using SemanticKernelChat.Infrastructure;
using Spectre.Console.Cli;

namespace SemanticKernelChat.Commands;

public sealed class ChatStreamCommand : ChatCommandBase
{
    public ChatStreamCommand(
        IChatClient chatClient,
        IChatHistoryService history,
        IChatController controller,
        IChatConsole console,
        McpToolCollection toolCollection)
        : base(chatClient, history, controller, console, toolCollection)
    {
    }

    protected override Task SendAndDisplayAsync(
        IChatClient chatClient,
        IChatHistoryService history,
        IReadOnlyList<McpClientTool> tools) =>
        Controller.SendAndDisplayStreamingAsync(chatClient, history, tools);
}
