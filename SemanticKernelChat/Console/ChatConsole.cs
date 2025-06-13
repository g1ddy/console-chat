using System;
using System.Text;

using Microsoft.Extensions.AI;

using ModelContextProtocol.Client;

using RadLine;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace SemanticKernelChat.Console;

internal static class ChatConsole
{
    private static LineEditor? _editor;
    private static readonly Style UserPanelStyle = new(Color.RoyalBlue1);
    private static readonly Style AssistantPanelStyle = new(Color.DarkSeaGreen2);
    private static readonly Style ToolPanelStyle = new(Color.Grey37);

    public static void Initialize(IEnumerable<McpClientTool> tools)
    {
        var completion = new CommandCompletion(tools.Select(t => t.Name));
        _editor = new LineEditor
        {
            MultiLine = true,
            Completion = completion,
        };
    }

    internal static (Style style, string headerText) GetPanelConfig(ChatRole messageRole)
    {
        var style = Style.Plain;
        var headerText = messageRole.ToString();
        if (messageRole == ChatRole.User)
        {
            style = UserPanelStyle;
            headerText = ":bust_in_silhouette: User";
        }
        else if (messageRole == ChatRole.Assistant)
        {
            style = AssistantPanelStyle;
            headerText = ":robot: Assistant";
        }
        else if (messageRole == ChatRole.Tool)
        {
            style = ToolPanelStyle;
            headerText = ":wrench: Tool";
        }

        return (style, headerText);
    }

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

            var (style, headerText) = GetPanelConfig(message.Role);
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
    /// Reads user input using RadLine's multiline editor.
    /// Returns <c>null</c> when the input stream ends.
    /// </summary>
    public static async Task<string?> ReadMultilineInputAsync()
    {
        _editor ??= new LineEditor { MultiLine = true };
        return await _editor.ReadLine(CancellationToken.None);
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

        var (style, headerText) = GetPanelConfig(ChatRole.Assistant);
        var header = new PanelHeader(headerText, Justify.Right);
        var panel = new Panel(string.Empty)
            .RoundedBorder()
            .BorderStyle(style)
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
                                .BorderStyle(style)
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

    private sealed class CommandCompletion : ITextCompletion
    {
        private static readonly string[] _base = new[] { "help", "exit", "enable", "disable", "toggle" };
        private readonly List<string> _toolNames;

        public CommandCompletion(IEnumerable<string> toolNames)
        {
            _toolNames = toolNames.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        }

        public IEnumerable<string>? GetCompletions(string prefix, string word, string suffix)
        {
            var tokens = (prefix + word).TrimStart().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length <= 1)
            {
                return _base.Concat(_toolNames).Where(c => c.StartsWith(word, StringComparison.OrdinalIgnoreCase));
            }

            var cmd = tokens[0];
            if ((cmd.Equals("enable", StringComparison.OrdinalIgnoreCase) ||
                cmd.Equals("disable", StringComparison.OrdinalIgnoreCase) ||
                cmd.Equals("toggle", StringComparison.OrdinalIgnoreCase)) &&
                tokens.Length == 2)
            {
                var part = tokens[1];
                return _toolNames.Where(t => t.StartsWith(part, StringComparison.OrdinalIgnoreCase));
            }

            return null;
        }
    }
}
