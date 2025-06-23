using SemanticKernelChat;
using SemanticKernelChat.Console;
using System.Collections.Generic;
using Spectre.Console.Cli;

namespace SemanticKernelChat.Commands;

public sealed class ChatStreamCommand : ChatCommandBase
{
    public ChatStreamCommand(
        IChatHistoryService history,
        IChatController controller,
        IChatConsole console,
        IEnumerable<IChatCommandStrategy> strategies)
        : base(history, controller, console, strategies)
    {
    }

    protected override Task SendAndDisplayAsync() =>
        Controller.SendAndDisplayStreamingAsync(History);
}
