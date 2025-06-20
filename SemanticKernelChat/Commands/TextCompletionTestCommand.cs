using Microsoft.SemanticKernel;
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
    private readonly Kernel _kernel;

    public TextCompletionTestCommand(
        IChatClient chatClient,
        IChatHistoryService history,
        IChatController controller,
        IChatConsole console)
    {
        _chatClient = chatClient;
        _history = history;
        _controller = controller;
        _console = console;
        _kernel = Kernel.CreateBuilder().Build();
    }

    public override async Task<int> ExecuteAsync(CommandContext context)
    {
        await using var toolCollection = await McpToolCollection.CreateAsync();

#pragma warning disable SKEXP0001 // Experimental API
        _ = _kernel.ImportPluginFromFunctions(
            "mcp",
            toolCollection.Tools.Select(t => t.AsKernelFunction()));
#pragma warning restore SKEXP0001

        var tools = toolCollection.Tools;
        _console.Initialize(tools);

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
