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

    public static (string headerText, Justify justify, Style style) GetUserStyle(ChatRole? messageRole)
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

    private static string? SerializeArguments(IDictionary<string, object?>? args)
    {
        return args?.Count > 0 ? JsonSerializer.Serialize(args) : null;
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
            var renderables = new List<IRenderable> { new Markup(markupText) };
            var contents = message.Contents;

            if (DebugEnabled)
            {
                foreach (var call in contents.OfType<FunctionCallContent>())
                {
                    string toolName = GetToolName(callNames, call.CallId, message.AuthorName, message.Role);
                    string toolMarkup = $"[grey]:wrench: {toolName.EscapeMarkup()} Parameters...[/]";
                    renderables.AddRange(FormatPanelContent(toolMarkup, SerializeArguments(call.Arguments)));
                }
            }

            if (message.Role == ChatRole.Tool)
            {
                foreach (var result in contents.OfType<FunctionResultContent>())
                {
                    string toolName = GetToolName(callNames, result.CallId, message.AuthorName, message.Role);
                    string toolMarkup = $"[grey]:wrench: {toolName.EscapeMarkup()} Result...[/]";
                    renderables.AddRange(FormatPanelContent(toolMarkup, result.Result?.ToString()));
                }
            }

            var (headerText, justify, style) = GetUserStyle(message.Role);
            var header = new PanelHeader(headerText, justify);
            var rows = new Rows(renderables);

            _console.Write(
                new Panel(rows)
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
        var renderables = new List<Panel>();
        var rows = new Rows(renderables);

        var callNames = new Dictionary<string, string>();
        await _console.Live(rows)
            .AutoClear(false)
            .StartAsync(async ctx =>
            {
                await foreach (var update in updates)
                {
                    messageUpdates.Add(update);
                    AppendUpdate(renderables, textBuilder, callNames, update);
                    rows = new Rows(renderables);
                    ctx.UpdateTarget(rows);
                    ctx.Refresh();
                }
            });

        var response = Microsoft.Extensions.AI.ChatResponseExtensions.ToChatResponse(messageUpdates);

        return [.. response.Messages];
    }

    private void AppendUpdate(
        List<Panel> panels,
        StringBuilder textBuilder,
        Dictionary<string, string> callNames,
        ChatResponseUpdate update)
    {
        var contents = update.Contents ?? Array.Empty<AIContent>();
        CollectFunctionCallNames(contents, callNames);

        _ = textBuilder.Append(update.Text?.EscapeMarkup() ?? string.Empty);

        var (headerText, justify, style) = GetUserStyle(update.Role);
        var header = new PanelHeader(headerText, justify);

        if (update.Role == ChatRole.Assistant)
        {
            var markupText = new Markup(textBuilder.ToString());
            var assistantPanel = new Panel(markupText)
                .RoundedBorder()
                .BorderStyle(style)
                .Header(header)
                .Expand();

            // update first or default or insert new row to panels
            if (panels.Count > 0)
            {
                panels[0] = assistantPanel;
            }
            else
            {
                panels.Add(assistantPanel);
            }
        }

        var renderables = new List<IRenderable>();

        if (DebugEnabled)
        {
            foreach (var call in contents.OfType<FunctionCallContent>())
            {
                string toolName = GetToolName(callNames, call.CallId, update.AuthorName, update.Role);
                string toolMarkup = $"[grey]:wrench: {toolName.EscapeMarkup()} Parameters...[/]";
                renderables.AddRange(FormatPanelContent(toolMarkup, SerializeArguments(call.Arguments)));
            }
        }

        if (update.Role == ChatRole.Tool)
        {
            foreach (var result in contents.OfType<FunctionResultContent>())
            {
                string toolName = GetToolName(callNames, result.CallId, update.AuthorName, update.Role);
                string toolMarkup = $"[grey]:wrench: {toolName.EscapeMarkup()} Result...[/]";
                renderables.AddRange(FormatPanelContent(toolMarkup, result.Result?.ToString()));
            }
        }

        if (renderables.Any())
        {
            var rows = new Rows(renderables);
            var toolsPanel = new Panel(rows)
                .RoundedBorder()
                .BorderStyle(style)
                .Header(header)
                .Expand();

            panels.Add(toolsPanel);
        }
    }
}
