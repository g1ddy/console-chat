using Microsoft.Extensions.AI;
using System.Collections.Generic;
using System.Linq;

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
    private readonly List<IChatCommandStrategy> _commands;

    protected ChatCommandBase(
        IChatHistoryService history,
        IChatController controller,
        IChatConsole console,
        IEnumerable<IChatCommandStrategy> commands)
    {
        _history = history;
        Controller = controller;
        _console = console;
        _commands = commands.ToList();
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

            if (input is null)
            {
                _console.WriteExitMessage();
                break;
            }

            if (string.IsNullOrWhiteSpace(input))
            {
                continue;
            }

            foreach (var cmd in _commands)
            {
                if (cmd.CanExecute(input))
                {
                    bool cont = await cmd.ExecuteAsync(input, _history, Controller, _console);
                    if (!cont)
                        return 0;
                    goto ContinueLoop;
                }
            }

            _history.AddUserMessage(input);

            await SendAndDisplayAsync();

ContinueLoop:;
        }

        return 0;
    }
}
