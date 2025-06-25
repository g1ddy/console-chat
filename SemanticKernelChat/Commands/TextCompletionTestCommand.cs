using Microsoft.Extensions.AI;
using SemanticKernelChat.Console;
using SemanticKernelChat.Infrastructure;
using Spectre.Console;
using Spectre.Console.Cli;

namespace SemanticKernelChat.Commands;

/// <summary>
/// Demonstration command that runs through a scripted chat interaction. It uses
/// the standard chat loop from <see cref="ChatCommandBase"/> but feeds the user
/// input from a mocked line editor so the conversation runs automatically.
/// </summary>
public sealed class TextCompletionTestCommand : ChatCommandBase
{
    /// <summary>
    /// Simple line editor returning predefined input lines.
    /// </summary>
    private sealed class FakeLineEditor : IChatLineEditor
    {
        private readonly Queue<string?> _inputs;

        public FakeLineEditor(IEnumerable<string?> inputs)
        {
            _inputs = new Queue<string?>(inputs);
        }

        public Task<string?> ReadLine(CancellationToken cancellationToken)
            => Task.FromResult(_inputs.Count > 0 ? _inputs.Dequeue() : null);
    }

    private static readonly string[] ScriptedInputs =
    [
        "This is a demo of non-streaming chat!",
        "This is a demo of streaming chat!",
        $"{CliConstants.Commands.List} {CliConstants.Options.Tools}",
        $"{CliConstants.Commands.List} {CliConstants.Options.Prompts}",
        CliConstants.Commands.Exit
    ];

    private int _sendCount;

    public TextCompletionTestCommand(
        IChatHistoryService history,
        IChatClient chatClient,
        McpToolCollection tools,
        IEnumerable<IChatCommandStrategy> strategies,
        IAnsiConsole ansiConsole)
        : base(
            history,
            CreateController(chatClient, tools, ansiConsole, out var console),
            console,
            strategies)
    {
    }

    private static IChatController CreateController(
        IChatClient chatClient,
        McpToolCollection tools,
        IAnsiConsole ansiConsole,
        out IChatConsole console)
    {
        var chatConsole = new ChatConsole(new FakeLineEditor(ScriptedInputs), ansiConsole);
        console = chatConsole;
        return new ChatController(chatConsole, chatClient, tools);
    }

    /// <summary>
    /// Sends the next chat message in the scripted sequence using both
    /// non-streaming and streaming display methods.
    /// </summary>
    protected override async Task SendAndDisplayAsync()
    {
        if (_sendCount == 0)
        {
            await Controller.SendAndDisplayAsync(History);
        }
        else
        {
            await Controller.SendAndDisplayStreamingAsync(History);
        }

        _sendCount++;
    }
}
