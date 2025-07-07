using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using System.Text.Json;

using Microsoft.Extensions.AI;

using Spectre.Console;
using Spectre.Console.Json;
using Spectre.Console.Rendering;

namespace SemanticKernelChat.Console;

public class ChatConsole : IChatConsole
{
    private readonly IChatLineEditor _editor;
    private readonly IAnsiConsole _console;
    public bool DebugEnabled { get; set; }

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

    private static string GetToolName(
        Dictionary<string, string> callNames,
        string? callId,
        string? authorName,
        ChatRole? role)
    {
        var defaultName = authorName ??
            CultureInfo.InvariantCulture.TextInfo.ToTitleCase(role?.ToString() ?? string.Empty);

        return (!string.IsNullOrEmpty(callId) && callNames.TryGetValue(callId, out var nameFound))
            ? nameFound
            : defaultName;
    }

    private static bool IsValidJson(string text)
    {
        try
        {
            _ = JsonDocument.Parse(text);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private IEnumerable<IRenderable> FormatPanelContent(string markup, string? rawJson)
    {
        var rows = new List<IRenderable> { new Markup(markup) };

        if (DebugEnabled && !string.IsNullOrWhiteSpace(rawJson))
        {
            if (IsValidJson(rawJson))
            {
                rows.Add(new JsonText(rawJson));
            }
            else
            {
                rows.Add(new Markup($"[orange1]:warning: {Markup.Escape(rawJson)}[/]"));
            }
        }

        return rows;
    }

    public void WriteChatMessages(params ChatMessage[] messages)
    {
        var callNames = CollectFunctionCallNames(messages);

        foreach (var message in messages)
        {
            string markupText = message.Text?.EscapeMarkup() ?? string.Empty;
            string? toolResultRaw = null;
            if (message.Role == ChatRole.Tool &&
                TryGetContent<FunctionResultContent>(message.Contents, out var result))
            {
                string toolName = GetToolName(callNames, result.CallId, message.AuthorName, message.Role);
                markupText = $"[grey]:wrench: {toolName.EscapeMarkup()} Result...[/]";
                toolResultRaw = result.Result?.ToString();
            }

            var rows = FormatPanelContent(markupText, toolResultRaw);
            IRenderable markupResponse = new Rows(rows);

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

    public void Write(IRenderable renderable) => _console.Write(renderable);

    public IReadOnlyList<string> PromptMultiSelection(string title, IEnumerable<(string Name, bool Selected)> items)
    {
        var prompt = new MultiSelectionPrompt<(string Name, bool Selected)>()
            .Title(title)
            .InstructionsText("[grey](Press <space> to toggle, <enter> to accept)[/]")
            .UseConverter(c => $"{c.Name} {(c.Selected ? "[green](enabled)[/]" : "[red](disabled)[/]")}");

        foreach (var item in items)
        {
            _ = prompt.AddChoice(item);
            if (item.Selected)
            {
                _ = prompt.Select(item);
            }
        }

        var result = _console.Prompt(prompt);
        _console.Clear();

        if (result.Count > 0)
        {
            var table = new Table().Border(TableBorder.Rounded).BorderColor(Color.Grey);
            _ = table.AddColumn("[bold]Selected[/]");

            foreach (var selection in result)
            {
                _ = table.AddRow($"[yellow]*[/] {selection.Name.EscapeMarkup()}");
            }

            _console.Write(table);
        }
        else
        {
            _console.MarkupLine("[grey]No selections made.[/]");
        }

        return result.Select(selection => selection.Name).ToList();
    }

    /// <summary>
    /// Reads user input using RadLine's multiline editor.
    /// Returns <c>null</c> when the input stream ends.
    /// </summary>
    public async Task<string?> ReadMultilineInputAsync(CancellationToken cancellationToken = default)
        => await _editor.ReadLine(CancellationToken.None);

    public async Task DisplayThinkingIndicator(Func<Task> action)
    {
        await _console.Status()
            .Spinner(Spinner.Known.Monkey)
            .StartAsync("Thinking...", async _ => await action());
    }

    public void DisplayError(Exception ex)
        => _console.WriteException(ex, ExceptionFormats.ShortenEverything);

    public async Task<IReadOnlyList<ChatMessage>> DisplayStreamingUpdatesAsync(
        IAsyncEnumerable<ChatResponseUpdate> updates)
    {
        var messageUpdates = new List<ChatResponseUpdate>();
        var textBuilder = new StringBuilder();
        var rows = new List<IRenderable> { new Markup(string.Empty) };
        var panel = CreateAssistantPanel(new Rows(rows));

        var callNames = new Dictionary<string, string>();
        await _console.Live(panel)
            .AutoClear(false)
            .StartAsync(async ctx =>
            {
                await foreach (var update in updates)
                {
                    messageUpdates.Add(update);
                    AppendUpdate(rows, textBuilder, callNames, update);
                    panel = CreateAssistantPanel(new Rows(rows));
                    ctx.UpdateTarget(panel);
                    ctx.Refresh();
                }
            });

        var response = Microsoft.Extensions.AI.ChatResponseExtensions.ToChatResponse(messageUpdates);

        return [.. response.Messages];
    }

    private static Panel CreateAssistantPanel(IRenderable content)
    {
        var (headerText, justify, style) = GetUserStyle(ChatRole.Assistant);
        var header = new PanelHeader(headerText, justify);

        return new Panel(content)
            .RoundedBorder()
            .BorderStyle(style)
            .Header(header)
            .Expand();
    }

    private void AppendUpdate(
        List<IRenderable> rows,
        StringBuilder textBuilder,
        Dictionary<string, string> callNames,
        ChatResponseUpdate update)
    {
        var contents = update.Contents ?? Array.Empty<AIContent>();
        CollectFunctionCallNames(contents, callNames);

        if (update.Role == ChatRole.Assistant)
        {
            _ = textBuilder.Append(update.Text?.EscapeMarkup() ?? string.Empty);
            rows[0] = new Markup(textBuilder.ToString());
        }

        foreach (var result in contents.OfType<FunctionResultContent>())
        {
            string toolName = GetToolName(callNames, result.CallId, update.AuthorName, update.Role);
            string toolMarkup = $"[grey]:wrench: {toolName.EscapeMarkup()} Result...[/]";
            rows.AddRange(FormatPanelContent(toolMarkup, result.Result?.ToString()));
        }
    }
}
