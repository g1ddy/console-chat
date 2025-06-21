using Microsoft.Extensions.AI;

using ModelContextProtocol.Client;

using SemanticKernelChat.Console;
using SemanticKernelChat.Infrastructure;

using Spectre.Console;
using Spectre.Console.Cli;

namespace SemanticKernelChat.Commands;

/// <summary>
/// Base command providing the interactive chat loop used by chat commands.
/// Derived classes just supply the method that sends chat messages.
/// </summary>
public abstract class ChatCommandBase : AsyncCommand<ChatCommandBase.Settings>
{
    /// <summary>
    /// Shared command settings. Currently empty but reserved for future options.
    /// </summary>
    public sealed class Settings : CommandSettings
    {
    }

    private readonly IChatClient _chatClient;
    private readonly IChatHistoryService _history;
    private readonly McpToolCollection _toolCollection;
    protected IChatController Controller { get; }
    private readonly IChatConsole _console;
    private readonly IAnsiConsole _ansiConsole;

    protected ChatCommandBase(
        IChatClient chatClient,
        IChatHistoryService history,
        IChatController controller,
        IChatConsole console,
        IAnsiConsole ansiConsole,
        McpToolCollection toolCollection)
    {
        _chatClient = chatClient;
        _history = history;
        Controller = controller;
        _console = console;
        _ansiConsole = ansiConsole;
        _toolCollection = toolCollection;
    }

    /// <summary>
    /// Implemented by derived commands to send and display chat responses.
    /// </summary>
    protected abstract Task SendAndDisplayAsync(
        IChatClient chatClient,
        IChatHistoryService history,
        IReadOnlyList<McpClientTool> tools);

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        var tools = _toolCollection.Tools;

        _ansiConsole.MarkupLine(CliConstants.WelcomeMessage);

        while (true)
        {
            var (headerText, justify, style) = ChatConsole.GetUserStyle(ChatRole.User);
            var rule = new Rule(headerText) { Justification = justify, Style = style };
            _ansiConsole.Write(rule);

            _ansiConsole.Markup(CliConstants.UserPrompt);
            var input = await _console.ReadMultilineInputAsync();

            if (input is null ||
                input.Equals(CliConstants.Commands.Exit, StringComparison.OrdinalIgnoreCase))
            {
                _ansiConsole.MarkupLine(CliConstants.ExitMessage);
                break;
            }

            if (string.IsNullOrWhiteSpace(input))
            {
                continue;
            }

            _history.AddUserMessage(input);

            await SendAndDisplayAsync(_chatClient, _history, tools);
        }

        return 0;
    }
}
