using System;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Client;
using Spectre.Console;
using Spectre.Console.Cli;

namespace SemanticKernelChat;

public sealed class ChatStreamCommand : AsyncCommand<ChatCommand.Settings>
{
    private readonly IChatClient _chatClient;
    private readonly IChatHistoryService _history;
    private readonly ILogger<ChatStreamCommand> _logger;

    public ChatStreamCommand(IChatClient chatClient, IChatHistoryService history, ILogger<ChatStreamCommand> logger)
    {
        _chatClient = chatClient;
        _history = history;
        _logger = logger;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, ChatCommand.Settings settings)
    {
        await using var toolCollection = await McpToolCollection.CreateAsync();
        var tools = toolCollection.Tools;

        AnsiConsole.MarkupLine("Type 'exit' to quit.");

        while (true)
        {
            AnsiConsole.Markup("You: ");
            var input = ChatCommand.ReadMultilineInput();
            if (string.IsNullOrWhiteSpace(input))
            {
                continue;
            }
            if (input.Equals("exit", StringComparison.OrdinalIgnoreCase))
            {
                break;
            }

            await SendAndDisplayStreamingAsync(input, tools);
        }

        return 0;
    }

    private async Task SendAndDisplayStreamingAsync(string input, IReadOnlyList<McpClientTool> tools)
    {
        _history.AddUserMessage(input);
        AnsiConsole.Write(
            new Panel(input)
                .RoundedBorder()
                .Header(new PanelHeader("You"))
                .Expand());

        var replyBuilder = new System.Text.StringBuilder();
        Exception? error = null;
        var panel = new Panel(string.Empty)
            .RoundedBorder()
            .Header(new PanelHeader("AI", Justify.Right))
            .Expand();

        await AnsiConsole.Live(panel)
            .AutoClear(false)
            .StartAsync(async ctx =>
            {
                await AnsiConsole.Status()
                    .Spinner(Spinner.Known.Monkey)
                    .StartAsync("Thinking...", async _ =>
                    {
                        try
                        {
                            await foreach (var update in _chatClient.GetStreamingResponseAsync(
                                _history.Messages,
                                new() { Tools = [.. tools] }))
                            {
                                replyBuilder.Append(update.Text);
                                panel = new Panel(replyBuilder.ToString())
                                    .RoundedBorder()
                                    .Header(new PanelHeader("AI", Justify.Right))
                                    .Expand();
                                ctx.UpdateTarget(panel);
                            }
                        }
                        catch (Exception ex)
                        {
                            error = ex;
                        }
                    });
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

        var reply = replyBuilder.ToString();
        _history.AddAssistantMessage(reply);
    }
}
