using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;
using SemanticKernelChat.Infrastructure;
using SemanticKernelChat.Console;
using Spectre.Console.Cli;

namespace SemanticKernelChat.Commands;

public sealed class ChatStreamCommand : ChatCommandBase
{
    public ChatStreamCommand(IChatClient chatClient, IChatHistoryService history)
        : base(chatClient, history)
    {
    }

    protected override Task SendAndDisplayAsync(
        IChatClient chatClient,
        IChatHistoryService history,
        IReadOnlyList<McpClientTool> tools) =>
        ChatController.SendAndDisplayStreamingAsync(chatClient, history, tools);
}
