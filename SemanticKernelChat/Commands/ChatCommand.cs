using SemanticKernelChat.Console;

namespace SemanticKernelChat.Commands;

public sealed class ChatCommand : ChatCommandBase
{
    public ChatCommand(
        IChatHistoryService history,
        IChatController controller,
        IChatConsole console,
        IEnumerable<IChatCommandStrategy> strategies)
        : base(history, controller, console, strategies)
    {
    }

    protected override Task SendAndDisplayAsync()
        => Controller.SendAndDisplayAsync(History);
}
