using System;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Client;
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

            await SendAndDisplayAsync(input, tools);
        }

        return 0;
    }

    private async Task SendAndDisplayAsync(string input, IReadOnlyList<McpClientTool> tools)
    {
        _history.AddUserMessage(input);
        AnsiConsole.Write(
            new Panel(input)
                .RoundedBorder()
                .Header(new PanelHeader("You"))
                .Expand());

        string reply = string.Empty;
        Exception? error = null;
        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Monkey)
            .StartAsync("Thinking...", async _ =>
            {
                try
                {
                    var response = await _chatClient.GetResponseAsync(
                        _history.Messages,
                        new() { Tools = [.. tools] });
                    reply = response.Text;
                }
                catch (Exception ex)
                {
                    error = ex;
                }
            });

        if (error is not null)
        {
            AnsiConsole.Write(
                new Panel(error.ToString())
                    .RoundedBorder()
                    .Header(new PanelHeader("Error"))
                    .Expand());
            return;
        }

        _history.AddAssistantMessage(reply);

        AnsiConsole.Write(
            new Panel(reply)
                .RoundedBorder()
                .Header(new PanelHeader("AI", Justify.Right))
                .Expand());
    }

    internal static string ReadMultilineInput()
    {
        // Console.ReadLine returns null if the input stream has ended (e.g. Ctrl+D)
        // and an empty string when the user just presses Enter.
        var lines = new List<string>();
        string? line;
        while ((line = Console.ReadLine()) != null)
        {
            if (string.IsNullOrEmpty(line))
            {
                if (lines.Count > 0)
                {
                    break;
                }
                continue;
            }

            lines.Add(line);
        }

        return string.Join(Environment.NewLine, lines);
    }
}

