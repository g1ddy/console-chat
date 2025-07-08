using System.Diagnostics.CodeAnalysis;
using System.Text;

using Microsoft.Extensions.AI;

using Spectre.Console;
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

    private void AddDebugPanels(
        string? authorName,
        ChatRole? role,
        IEnumerable<FunctionCallContent> callContents,
        IEnumerable<FunctionResultContent> resultContents,
        Dictionary<string, string> callNames,
        List<IRenderable> renderables)
    {
        if (DebugEnabled)
        {
            foreach (var call in callContents)
            {
                string toolName = ChatConsoleHelpers.GetToolName(callNames, call.CallId, authorName, role);
                string toolMarkup = $"[grey]:wrench: {toolName.EscapeMarkup()} Parameters...[/]";
                renderables.AddRange(ChatConsoleHelpers.FormatPanelContent(toolMarkup, ChatConsoleHelpers.SerializeArguments(call.Arguments), DebugEnabled));
            }
        }

        if (role == ChatRole.Tool)
        {
            foreach (var result in resultContents)
            {
                string toolName = ChatConsoleHelpers.GetToolName(callNames, result.CallId, authorName, role);
                string toolMarkup = $"[grey]:wrench: {toolName.EscapeMarkup()} Result...[/]";
                renderables.AddRange(ChatConsoleHelpers.FormatPanelContent(toolMarkup, result.Result?.ToString(), DebugEnabled));
            }
        }
    }

    public void WriteChatMessages(params ChatMessage[] messages)
    {
        var callNames = ChatConsoleHelpers.CollectFunctionCallNames(messages);

        foreach (var message in messages)
        {
            string markupText = message.Text?.EscapeMarkup() ?? string.Empty;
            var renderables = new List<IRenderable> { new Markup(markupText) };
            var contents = message.Contents;
            AddDebugPanels(
                message.AuthorName,
                message.Role,
                contents.OfType<FunctionCallContent>(),
                contents.OfType<FunctionResultContent>(),
                callNames,
                renderables);

            var (headerText, justify, style) = ChatConsoleHelpers.GetHeaderStyle(message.Role);
            var header = new PanelHeader(headerText, justify);
            var rows = new Rows(renderables);
            _console.Write(ChatConsoleHelpers.CreatePanel(rows, style, header));
        }
    }

    public void WriteWelcomeMessage() => _console.MarkupLine(CliConstants.WelcomeMessage);

    public void WriteExitMessage() => _console.MarkupLine(CliConstants.ExitMessage);

    public void WriteHeader(ChatRole role)
    {
        var (headerText, justify, style) = ChatConsoleHelpers.GetHeaderStyle(role);
        var rule = new Rule(headerText) { Justification = justify, Style = style };
        _console.Write(rule);
    }

    public void WriteUserPrompt() => _console.Markup(CliConstants.UserPrompt);

    public void WriteLine(string text) => _console.WriteLine(text);

    public void Write(IRenderable renderable) => _console.Write(renderable);

    public void WritePanel(IRenderable content, string title)
    {
        var header = new PanelHeader(title);
        var panel = ChatConsoleHelpers.CreatePanel(content, Style.Plain, header);
        _console.Write(panel);
    }

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
        ChatConsoleHelpers.CollectFunctionCallNames(contents, callNames);

        _ = textBuilder.Append(update.Text?.EscapeMarkup() ?? string.Empty);

        var (headerText, justify, style) = ChatConsoleHelpers.GetHeaderStyle(update.Role);
        var header = new PanelHeader(headerText, justify);

        if (update.Role == ChatRole.Assistant)
        {
            var markupText = new Markup(textBuilder.ToString());
            var assistantPanel = ChatConsoleHelpers.CreatePanel(markupText, style, header);

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
        AddDebugPanels(
            update.AuthorName,
            update.Role,
            contents.OfType<FunctionCallContent>(),
            contents.OfType<FunctionResultContent>(),
            callNames,
            renderables);

        if (renderables.Any())
        {
            var rows = new Rows(renderables);
            var toolsPanel = ChatConsoleHelpers.CreatePanel(rows, style, header);
            panels.Add(toolsPanel);
        }
    }
}
