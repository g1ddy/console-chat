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
        var header = message.Role == ChatRole.Assistant
            ? new PanelHeader("AI", Justify.Right)
            : new PanelHeader("You");

        AnsiConsole.Write(
            new Panel(message.Text)
                .RoundedBorder()
                .Header(header)
                .Expand());
    }

    public static async Task SendAndDisplayAsync(
        IChatClient chatClient,
        IChatHistoryService history,
        string input,
        IReadOnlyList<McpClientTool> tools)
    {
        history.AddUserMessage(input);
        WriteChatMessage(new ChatMessage(ChatRole.User, input));

        string reply = string.Empty;
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
                    reply = response.Text;
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

        history.AddAssistantMessage(reply);
        WriteChatMessage(new ChatMessage(ChatRole.Assistant, reply));
    }

    public static async Task SendAndDisplayStreamingAsync(
        IChatClient chatClient,
        IChatHistoryService history,
        string input,
        IReadOnlyList<McpClientTool> tools)
    {
        history.AddUserMessage(input);
        WriteChatMessage(new ChatMessage(ChatRole.User, input));

        var replyBuilder = new StringBuilder();
        Exception? error = null;
        var header = new PanelHeader("AI", Justify.Right);
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
                        replyBuilder.Append(update.Text);
                        panel = new Panel(replyBuilder.ToString())
                            .RoundedBorder()
                            .Header(header)
                            .Expand();
                        ctx.UpdateTarget(panel);
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
        history.AddAssistantMessage(reply);
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
