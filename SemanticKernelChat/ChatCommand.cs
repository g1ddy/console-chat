using System;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using Spectre.Console.Cli;

namespace SemanticKernelChat;

public sealed class ChatCommand : AsyncCommand<ChatCommand.Settings>
{
    private readonly IChatClient _chatClient;
    private readonly IChatHistoryService _history;
    private readonly ILogger<ChatCommand> _logger;

    public sealed class Settings : CommandSettings
    {
    }

    public ChatCommand(IChatClient chatClient, IChatHistoryService history, ILogger<ChatCommand> logger)
    {
        _chatClient = chatClient;
        _history = history;
        _logger = logger;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        await using var toolCollection = await McpToolCollection.CreateAsync();
        var tools = toolCollection.Tools;

        AnsiConsole.MarkupLine("Type 'exit' to quit.");

        while (true)
        {
            AnsiConsole.Markup("You: ");
            var input = ReadMultilineInput();
            if (string.IsNullOrWhiteSpace(input))
            {
                continue;
            }
            if (input.Equals("exit", StringComparison.OrdinalIgnoreCase))
            {
                break;
            }

            _history.AddUserMessage(input);
            var response = await _chatClient.GetResponseAsync(_history.Messages, new() { Tools = [.. tools] });
            var reply = response.Text;
            AnsiConsole.MarkupLine($"AI: {reply}");
            _history.AddAssistantMessage(reply);
        }

        return 0;
    }

    private static string ReadMultilineInput()
    {
        var console = AnsiConsole.Console;
        var sb = new System.Text.StringBuilder();
        while (true)
        {
            var key = console.Input.ReadKey(intercept: true);
            if (key == null)
            {
                continue;
            }

            if (key.Value.Key == ConsoleKey.Enter)
            {
                if (key.Value.Modifiers.HasFlag(ConsoleModifiers.Shift))
                {
                    console.WriteLine();
                    sb.AppendLine();
                    continue;
                }

                console.WriteLine();
                break;
            }

            if (key.Value.Key == ConsoleKey.Backspace)
            {
                if (sb.Length > 0)
                {
                    var lastChar = sb[^1];
                    sb.Length--;
                    if (!char.IsControl(lastChar))
                    {
                        console.Cursor.MoveLeft(1);
                        console.Write(" ");
                        console.Cursor.MoveLeft(1);
                    }
                }
                continue;
            }

            if (!char.IsControl(key.Value.KeyChar))
            {
                console.Write(key.Value.KeyChar.ToString());
                sb.Append(key.Value.KeyChar);
            }
        }
        return sb.ToString();
    }
}

