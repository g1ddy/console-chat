using SemanticKernelChat;
using SemanticKernelChat.Console;
using Spectre.Console.Cli;

namespace SemanticKernelChat.Commands;

public sealed class ChatCommand : ChatCommandBase
{
    public ChatCommand(
        IChatHistoryService history,
        IChatController controller,
        IChatConsole console)
        : base(history, controller, console)
    {
    }

    protected override Task SendAndDisplayAsync() =>
        Controller.SendAndDisplayAsync(History);
}
