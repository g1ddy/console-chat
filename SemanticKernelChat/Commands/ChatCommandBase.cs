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
        [CommandOption("--file <PATH>")]
        public string? FilePath { get; set; }
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

    /// <summary>
    /// Iterate through registered command strategies and execute the first one
    /// that can handle <paramref name="input"/>.
    /// </summary>
    /// <param name="input">User input line.</param>
    /// <returns>
    /// <c>null</c> if no command handled the input; otherwise the result of the
    /// executed command indicating whether processing should continue.
    /// </returns>
    private async Task<bool?> TryHandleCommandAsync(string input)
    {
        foreach (var cmd in _commands)
        {
            if (cmd.CanExecute(input))
            {
                return await cmd.ExecuteAsync(input, _history, Controller, _console);
            }
        }

        return null;
    }

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

            var commandResult = await TryHandleCommandAsync(input);

            if (commandResult.HasValue)
            {
                if (!commandResult.Value)
                    return 0;
                continue;
            }

            _history.AddUserMessage(input);

            await SendAndDisplayAsync();
        }

        return 0;
    }
}
