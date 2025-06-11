using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;
using Spectre.Console;

namespace SemanticKernelChat;

internal static class ChatConsole
{
    public static void WriteChatMessage(ChatMessage message)
    {
        if (message.Role == ChatRole.Tool)
        {
            if (!string.IsNullOrWhiteSpace(message.AuthorName))
            {
                AnsiConsole.MarkupLine($"[grey]Tool call: {message.AuthorName}[/]");
            }

            if (!string.IsNullOrWhiteSpace(message.Text))
            {
                AnsiConsole.MarkupLine($"[grey]{message.Text}[/]");
            }

            return;
        }

        var headerText = message.Role.ToString();
        var header = message.Role == ChatRole.Assistant
            ? new PanelHeader(headerText, Justify.Right)
            : new PanelHeader(headerText);

        AnsiConsole.Write(
            new Panel(message.Text)
                .RoundedBorder()
                .Header(header)
                .Expand());
    }

    public static void WriteChatMessages(IChatHistoryService history, IEnumerable<ChatMessage> messages)
    {
        foreach (var message in messages)
        {
            history.Add(message);
            WriteChatMessage(message);
        }
    }


    public static async Task SendAndDisplayAsync(
        IChatClient chatClient,
        IChatHistoryService history,
        string input,
        IReadOnlyList<McpClientTool> tools)
    {
        var userMessage = new ChatMessage(ChatRole.User, input);
        history.Add(userMessage);
        WriteChatMessage(userMessage);

        List<ChatMessage> responses = [];
        Exception? error = null;
        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Monkey)
            .StartAsync("Thinking...", async _ =>
            {
                try
                {
                    var response = await chatClient.GetResponseAsync(
                        history.Messages,
                        new() { Tools = [.. tools] });
                    responses.AddRange(response.Messages);
                }
                catch (Exception ex)
                {
                    error = ex;
                }
            });

        if (error is not null)
        {
            AnsiConsole.WriteException(error, ExceptionFormats.ShortenEverything);
            return;
        }

        WriteChatMessages(history, responses);
    }

    public static async Task SendAndDisplayStreamingAsync(
        IChatClient chatClient,
        IChatHistoryService history,
        string input,
        IReadOnlyList<McpClientTool> tools)
    {
        var userMessage = new ChatMessage(ChatRole.User, input);
        history.Add(userMessage);
        WriteChatMessage(userMessage);

        var replyBuilder = new StringBuilder();
        Exception? error = null;
        var toolNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var header = new PanelHeader(ChatRole.Assistant.ToString(), Justify.Right);
        var panel = new Panel(string.Empty)
            .RoundedBorder()
            .Header(header)
            .Expand();

        await AnsiConsole.Live(panel)
            .AutoClear(false)
            .StartAsync(async ctx =>
            {
                try
                {
                    await foreach (var update in chatClient.GetStreamingResponseAsync(
                        history.Messages,
                        new() { Tools = [.. tools] }))
                    {
                        if (update.Role == ChatRole.Tool && !string.IsNullOrWhiteSpace(update.AuthorName))
                        {
                            toolNames.Add(update.AuthorName!);
                        }
                        if (update.Role == ChatRole.Assistant)
                        {
                            replyBuilder.Append(update.Text);
                            panel = new Panel(replyBuilder.ToString())
                                .RoundedBorder()
                                .Header(header)
                                .Expand();
                            ctx.UpdateTarget(panel);
                        }
                    }
                }
                catch (Exception ex)
                {
                    error = ex;
                }
            });

        if (error is not null)
        {
            AnsiConsole.WriteException(error, ExceptionFormats.ShortenEverything);
            return;
        }

        var reply = replyBuilder.ToString();
        var assistantMessage = new ChatMessage(ChatRole.Assistant, reply);
        history.Add(assistantMessage);
        if (toolNames.Count > 0)
        {
            AnsiConsole.MarkupLine($"[grey]Tool calls: {string.Join(", ", toolNames)}[/]");
        }
    }

    /// <summary>
    /// Reads user text until a blank line is entered. Returns <c>null</c> when the input stream ends.
    /// An empty string means the user pressed Enter on an empty line.
    /// This keeps the implementation minimal at the cost of advanced editing features.
    /// </summary>
    public static string? ReadMultilineInput()
    {
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

        return line is null && lines.Count == 0
            ? null
            : string.Join(Environment.NewLine, lines);
    }
}
