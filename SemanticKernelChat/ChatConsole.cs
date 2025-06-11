using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.AI;

using ModelContextProtocol.Client;

using Spectre.Console;
using Spectre.Console.Rendering;

namespace SemanticKernelChat;

internal static class ChatConsole
{
    public static void WriteChatMessages(IChatHistoryService history, params ChatMessage[] messages)
    {
        foreach (var message in messages)
        {
            history.Add(message);

            IRenderable markupResponse = new Markup(message.Text.EscapeMarkup());
            if (message.Role == ChatRole.Tool)
            {
                var content = message.Contents.FirstOrDefault();
                if (content is FunctionResultContent functionResultContent)
                {
                    markupResponse = new Markup("[grey]tool: Tool Result...[/]");
                    //textResponse = new JsonJsonText(functionResultContent.Result?.ToString());
                }
            }

            var headerText = message.Role.ToString();
            var header = message.Role == ChatRole.Assistant
                ? new PanelHeader(headerText, Justify.Right)
                : new PanelHeader(headerText);

            AnsiConsole.Write(
                new Panel(markupResponse)
                    .RoundedBorder()
                    .Header(header)
                    .Expand());
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

        return string.Join(Environment.NewLine, lines);
    }

    public static async Task SendAndDisplayAsync(
        IChatClient chatClient,
        IChatHistoryService history,
        IReadOnlyList<McpClientTool> tools)
    {
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

        WriteChatMessages(history, responses.ToArray());
    }

    public static async Task SendAndDisplayStreamingAsync(
        IChatClient chatClient,
        IChatHistoryService history,
        IReadOnlyList<McpClientTool> tools)
    {
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
                        if (update.Role == ChatRole.Assistant)
                        {
                            replyBuilder.Append(update.Text);
                            panel = new Panel(replyBuilder.ToString())
                                .RoundedBorder()
                                .Header(header)
                                .Expand();
                            ctx.UpdateTarget(panel);
                        }
                        else if (update.Role == ChatRole.Tool)
                        {
                            replyBuilder.Append(Environment.NewLine);
                            replyBuilder.AppendFormat("[grey]{0}: Tool Result...[/]", update.Role);
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
    }
}
