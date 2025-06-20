using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;
using SemanticKernelChat.Infrastructure;
using SemanticKernelChat.Console;
using Spectre.Console;
using Spectre.Console.Cli;

namespace SemanticKernelChat.Commands;

public sealed class TextCompletionTestCommand : AsyncCommand
{
    private readonly IChatClient _chatClient;
    private readonly IChatHistoryService _history;
    private readonly IChatController _controller;
    private readonly IChatConsole _console;
    private readonly McpToolCollection _toolCollection;

    public TextCompletionTestCommand(
        IChatClient chatClient,
        IChatHistoryService history,
        IChatController controller,
        IChatConsole console,
        McpToolCollection toolCollection)
    {
        _chatClient = chatClient;
        _history = history;
        _controller = controller;
        _console = console;
        _toolCollection = toolCollection;
    }

    public override async Task<int> ExecuteAsync(CommandContext context)
    {
        var tools = _toolCollection.Tools;

        var (headerText, justify, style) = ChatConsole.GetUserStyle(ChatRole.User);
        var rule = new Rule(headerText) { Justification = justify, Style = style };
        AnsiConsole.Write(rule);
        AnsiConsole.Markup(CliConstants.UserPrompt);

        var input = "include ipsum loren text";
        _history.AddUserMessage(input);

        await _controller.SendAndDisplayAsync(_chatClient, _history, tools);
        await _controller.SendAndDisplayStreamingAsync(_chatClient, _history, tools);

        return 0;
    }
}
