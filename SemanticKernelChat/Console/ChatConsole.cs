using System.Diagnostics.CodeAnalysis;

using Microsoft.Extensions.AI;

using Spectre.Console;
using Spectre.Console.Rendering;

namespace SemanticKernelChat.Console;

public class ChatConsole : IChatConsole
{
    private readonly IChatLineEditor _editor;
    private readonly IAnsiConsole _console;

    public ChatConsole(IChatLineEditor editor, IAnsiConsole console)
    {
        _editor = editor;
        _console = console;
    }

    public static (string headerText, Justify justify, Style style) GetUserStyle(ChatRole messageRole)
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

    private static bool TryGetContent<TContent>(IEnumerable<AIContent> contents, [NotNullWhen(true)] out TContent? content)
        where TContent : AIContent
    {
        foreach (var item in contents)
        {
            if (item is TContent match)
            {
                content = match;
                return true;
            }
        }

        content = null;
        return false;
    }

    private static void CollectFunctionCallNames(IEnumerable<AIContent> contents, Dictionary<string, string> callNames)
    {
        foreach (var call in contents.OfType<FunctionCallContent>())
        {
            if (!string.IsNullOrEmpty(call.CallId) && !string.IsNullOrEmpty(call.Name))
            {
                callNames[call.CallId] = call.Name;
            }
        }
    }

    private static Dictionary<string, string> CollectFunctionCallNames(IEnumerable<ChatMessage> messages)
    {
        var callNames = new Dictionary<string, string>();
        foreach (var message in messages)
        {
            CollectFunctionCallNames(message.Contents, callNames);
        }

        return callNames;
    }

    public void WriteChatMessages(params ChatMessage[] messages)
    {
        var callNames = CollectFunctionCallNames(messages);

        foreach (var message in messages)
        {
            IRenderable markupResponse = new Markup(message.Text.EscapeMarkup());
            if (message.Role == ChatRole.Tool &&
                TryGetContent<FunctionResultContent>(message.Contents, out var result))
            {
                string toolName = message.AuthorName ?? message.Role.ToString();
                var id = result.CallId;
                if (!string.IsNullOrEmpty(id) && callNames.TryGetValue(id, out var nameFound))
                {
                    toolName = nameFound;
                }
                markupResponse = new Markup($"[grey]:wrench: {toolName} Result...[/]");
            }

            var (headerText, justify, style) = GetUserStyle(message.Role);
            var header = new PanelHeader(headerText, justify);

            _console.Write(
                new Panel(markupResponse)
                    .RoundedBorder()
                    .BorderStyle(style)
                    .Header(header)
                    .Expand());
        }
    }

    public void WriteWelcomeMessage() => _console.MarkupLine(CliConstants.WelcomeMessage);

    public void WriteExitMessage() => _console.MarkupLine(CliConstants.ExitMessage);

    public void WriteHeader(ChatRole role)
    {
        var (headerText, justify, style) = GetUserStyle(role);
        var rule = new Rule(headerText) { Justification = justify, Style = style };
        _console.Write(rule);
    }

    public void WriteUserPrompt() => _console.Markup(CliConstants.UserPrompt);

    public void WriteLine(string text) => _console.WriteLine(text);

    /// <summary>
    /// Reads user input using RadLine's multiline editor.
    /// Returns <c>null</c> when the input stream ends.
    /// </summary>
    public async Task<string?> ReadMultilineInputAsync()
    {
        return await _editor.ReadLine(CancellationToken.None);
    }

    public async Task DisplayThinkingIndicator(Func<Task> action)
    {
        await _console.Status()
            .Spinner(Spinner.Known.Monkey)
            .StartAsync("Thinking...", async _ => await action());
    }

    public void DisplayError(Exception ex)
    {
        _console.WriteException(ex, ExceptionFormats.ShortenEverything);
    }

    public async Task<IReadOnlyList<ChatMessage>> DisplayStreamingUpdatesAsync(
        IAsyncEnumerable<ChatResponseUpdate> updates)
    {
        var messageUpdates = new List<ChatResponseUpdate>();
        var paragraph = new Paragraph(string.Empty);
        var panel = CreateAssistantPanel(paragraph);

        var callNames = new Dictionary<string, string>();
        await _console.Live(panel)
            .AutoClear(false)
            .StartAsync(async ctx =>
            {
                await foreach (var update in updates)
                {
                    messageUpdates.Add(update);
                    AppendUpdate(paragraph, callNames, update);
                    ctx.Refresh();
                }
            });

        var response = Microsoft.Extensions.AI.ChatResponseExtensions.ToChatResponse(messageUpdates);
        return [.. response.Messages];
    }

    private static Panel CreateAssistantPanel(Paragraph paragraph)
    {
        var (headerText, justify, style) = GetUserStyle(ChatRole.Assistant);
        var header = new PanelHeader(headerText, justify);

        return new Panel(paragraph)
            .RoundedBorder()
            .BorderStyle(style)
            .Header(header)
            .Expand();
    }

    private static void AppendUpdate(
        Paragraph paragraph,
        Dictionary<string, string> callNames,
        ChatResponseUpdate update)
    {
        var contents = update.Contents ?? Array.Empty<AIContent>();
        CollectFunctionCallNames(contents, callNames);

        if (update.Role == ChatRole.Assistant)
        {
            _ = paragraph.Append(update.Text.EscapeMarkup());
        }

        foreach (var result in contents.OfType<FunctionResultContent>())
        {
            _ = paragraph.Append("\n");
            string toolName = update.AuthorName ?? update.Role.ToString();
            string? id = result.CallId;
            if (!string.IsNullOrEmpty(id) && callNames.TryGetValue(id, out var nameFound))
            {
                toolName = nameFound;
            }

            _ = paragraph.Append($"[grey]:wrench: {toolName} Result...[/]");
        }
    }
}
