using Microsoft.Extensions.AI;
using SemanticKernelChat.Console;
using Spectre.Console.Cli;

namespace SemanticKernelChat.Commands;

public sealed class TextCompletionTestCommand : AsyncCommand
{
    private readonly IChatHistoryService _history;
    private readonly IChatController _controller;
    private readonly IChatConsole _console;

    public TextCompletionTestCommand(
        IChatHistoryService history,
        IChatController controller,
        IChatConsole console)
    {
        _history = history;
        _controller = controller;
        _console = console;
    }

    public override async Task<int> ExecuteAsync(CommandContext context)
    {
        // Non-streaming chat demo
        _console.WriteHeader(ChatRole.User);
        _console.WriteUserPrompt();
        var inputNonStreaming = "This is a demo of non-streaming chat!";
        _console.WriteLine(inputNonStreaming);

        _history.AddUserMessage(inputNonStreaming);
        await _controller.SendAndDisplayAsync(_history);

        // Streaming chat demo
        _console.WriteHeader(ChatRole.User);
        _console.WriteUserPrompt();
        var inputStreaming = "This is a demo of streaming chat!";
        _console.WriteLine(inputStreaming);

        _history.AddUserMessage(inputStreaming);
        await _controller.SendAndDisplayStreamingAsync(_history);

        // /list command demo
        var listStrategy = new ListToolsCommandStrategy(_controller.ToolCollection);
        await listStrategy.ExecuteAsync("/list tools", _history, _controller, _console);

        // Fin
        return 0;
    }
}
