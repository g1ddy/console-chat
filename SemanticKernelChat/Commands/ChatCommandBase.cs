using Microsoft.Extensions.AI;

using SemanticKernelChat.Console;
using SemanticKernelChat.Infrastructure;
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

    private readonly IChatHistoryService _history;
    protected IChatHistoryService History => _history;
    protected IChatController Controller { get; }
    private readonly IChatConsole _console;

    protected ChatCommandBase(
        IChatHistoryService history,
        IChatController controller,
        IChatConsole console)
    {
        _history = history;
        Controller = controller;
        _console = console;
    }

    /// <summary>
    /// Implemented by derived commands to send and display chat responses.
    /// </summary>
    protected abstract Task SendAndDisplayAsync();

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        _console.WriteWelcomeMessage();

        while (true)
        {
            _console.WriteHeader(ChatRole.User);
            _console.WriteUserPrompt();
            var input = await _console.ReadMultilineInputAsync();

            if (input is null ||
                input.Equals(CliConstants.Commands.Exit, StringComparison.OrdinalIgnoreCase))
            {
                _console.WriteExitMessage();
                break;
            }

            if (string.IsNullOrWhiteSpace(input))
            {
                continue;
            }

            if (input.StartsWith(CliConstants.Commands.Toggle, StringComparison.OrdinalIgnoreCase))
            {
                var tokens = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (tokens.Length == 2 && tokens[1].Equals(CliConstants.Options.Mcp, StringComparison.OrdinalIgnoreCase))
                {
                    var tools = Controller.ToolCollection;
                    var choices = tools.Servers.Select(n => (Name: n, Selected: tools.IsServerEnabled(n)));
                    var selected = _console.PromptMultiSelection("Toggle MCP servers", choices).ToHashSet(StringComparer.OrdinalIgnoreCase);
                    foreach (var name in tools.Servers)
                    {
                        tools.SetServerEnabled(name, selected.Contains(name));
                    }
                    continue;
                }
            }

            if (input.StartsWith(CliConstants.Commands.Enable, StringComparison.OrdinalIgnoreCase) ||
                input.StartsWith(CliConstants.Commands.Disable, StringComparison.OrdinalIgnoreCase))
            {
                var tokens = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (tokens.Length == 3 && tokens[1].Equals(CliConstants.Options.Mcp, StringComparison.OrdinalIgnoreCase))
                {
                    var enable = tokens[0].Equals(CliConstants.Commands.Enable, StringComparison.OrdinalIgnoreCase);
                    var name = tokens[2];
                    Controller.ToolCollection.SetServerEnabled(name, enable);
                    continue;
                }
            }

            _history.AddUserMessage(input);

            await SendAndDisplayAsync();
        }

        return 0;
    }
}
