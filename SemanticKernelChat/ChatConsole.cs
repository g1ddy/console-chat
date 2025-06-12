using System.Text;

using Microsoft.Extensions.AI;

using ModelContextProtocol.Client;

using Spectre.Console;
using Spectre.Console.Rendering;

namespace SemanticKernelChat;

internal static class ChatConsole
{
    private static readonly Style UserPanelStyle = new(Color.White, Color.RoyalBlue1);
    private static readonly Style AssistantPanelStyle = new(Color.White, Color.DarkSeaGreen2);
    private static readonly Style ToolPanelStyle = new(Color.White, Color.Grey37);

    public static void WriteChatMessages(IChatHistoryService history, params ChatMessage[] messages)
    {
        history.Add(messages);

        foreach (var message in messages)
        {
            IRenderable markupResponse = new Markup(message.Text.EscapeMarkup());
            if (message.Role == ChatRole.Tool)
            {
                var content = message.Contents.FirstOrDefault();
                if (content is FunctionResultContent)
                {
                    markupResponse = new Markup("[grey]:wrench: Tool Result...[/]");
                }
            }

            var style = Style.Plain;
            var headerText = message.Role.ToString();
            if (message.Role == ChatRole.User)
            {
                style = UserPanelStyle;
                headerText = ":bust_in_silhouette: User";
            }
            else if (message.Role == ChatRole.Assistant)
            {
                style = AssistantPanelStyle;
                headerText = ":robot: Assistant";
            }
            else if (message.Role == ChatRole.Tool)
            {
                style = ToolPanelStyle;
                headerText = ":wrench: Tool";
            }

            var header = message.Role == ChatRole.Assistant
                ? new PanelHeader(headerText, Justify.Right)
                : new PanelHeader(headerText);

            AnsiConsole.Write(
                new Panel(markupResponse)
                    .RoundedBorder()
                    .BorderStyle(style)
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
        var messageUpdates = new List<ChatResponseUpdate>();
        var replyBuilder = new StringBuilder();
        Exception? error = null;

        var header = new PanelHeader(":robot: Assistant", Justify.Right);
        var panel = new Panel(string.Empty)
            .RoundedBorder()
            .BorderStyle(AssistantPanelStyle)
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
                        messageUpdates.Add(update);

                        if (update.Role == ChatRole.Assistant)
                        {
                            _ = replyBuilder.Append(update.Text.EscapeMarkup());
                            panel = new Panel(replyBuilder.ToString())
                                .RoundedBorder()
                                .BorderStyle(AssistantPanelStyle)
                                .Header(header)
                                .Expand();
                            ctx.UpdateTarget(panel);
                        }
                        else if (update.Role == ChatRole.Tool)
                        {
                            _ = replyBuilder.Append(Environment.NewLine);
                            _ = replyBuilder.AppendFormat("[grey]:wrench: {0} Result...[/]", update.Role);
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

        history.Add(messageUpdates.ToArray());
    }
}
