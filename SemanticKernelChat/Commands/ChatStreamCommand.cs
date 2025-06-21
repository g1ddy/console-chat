using SemanticKernelChat;
using SemanticKernelChat.Console;
using Spectre.Console.Cli;

namespace SemanticKernelChat.Commands;

public sealed class ChatStreamCommand : ChatCommandBase
{
    public ChatStreamCommand(
        IChatHistoryService history,
        IChatController controller,
        IChatConsole console)
        : base(history, controller, console)
    {
    }

    protected override Task SendAndDisplayAsync() =>
        Controller.SendAndDisplayStreamingAsync(History);
}
