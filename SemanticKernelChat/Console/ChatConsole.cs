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

    public static void Initialize(IEnumerable<McpClientTool> tools)
    {
        var completion = new CommandCompletion(tools.Select(t => t.Name));
        _editor = new LineEditor
        {
            MultiLine = true,
            Completion = completion,
        };
    }

    internal static (string headerText, Justify justify, Style style) GetUserStyle(ChatRole messageRole)
    {
        var headerText = messageRole.ToString();
        var justify = Justify.Left;
        var style = Style.Plain;

        if (messageRole == ChatRole.User)
        {
            headerText = ":bust_in_silhouette: User";
            style = new(Color.RoyalBlue1);
            justify = Justify.Left;
        }
        else if (messageRole == ChatRole.Assistant)
        {
            headerText = ":robot: Assistant";
            style = new(Color.DarkSeaGreen2);
            justify = Justify.Right;
        }
        else if (messageRole == ChatRole.Tool)
        {
            headerText = ":wrench: Tool";
            style = new(Color.Grey37);
            justify = Justify.Center;
        }

        headerText += $" | [grey]({DateTime.Now.ToShortTimeString()})[/]";

        return (headerText, justify, style);
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

            var (headerText, justify, style) = GetUserStyle(message.Role);
            var header = new PanelHeader(headerText, justify);

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

        var (headerText, justify, style) = GetUserStyle(ChatRole.Assistant);
        var header = new PanelHeader(headerText, justify);

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
        private static readonly string[] BaseCommands = { "help", "exit", "enable", "disable", "toggle" };
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
                return BaseCommands.Concat(_toolNames).Where(c => c.StartsWith(word, StringComparison.OrdinalIgnoreCase));
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
