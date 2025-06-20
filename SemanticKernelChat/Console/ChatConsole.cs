using System.Diagnostics.CodeAnalysis;

using Microsoft.Extensions.AI;

using Spectre.Console;
using Spectre.Console.Rendering;

namespace SemanticKernelChat.Console;

public class ChatConsole : IChatConsole
{
    private readonly IChatLineEditor _editor;

    public ChatConsole(IChatLineEditor editor)
    {
        _editor = editor;
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

    public void WriteChatMessages(params ChatMessage[] messages)
    {

        foreach (var message in messages)
        {
            IRenderable markupResponse = new Markup(message.Text.EscapeMarkup());
            if (message.Role == ChatRole.Tool &&
                TryGetContent<FunctionResultContent>(message.Contents, out _))
            {
                markupResponse = new Markup("[grey]:wrench: Tool Result...[/]");
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
    public async Task<string?> ReadMultilineInputAsync()
    {
        return await _editor.ReadLine(CancellationToken.None);
    }

    public async Task DisplayThinkingIndicator(Func<Task> action)
    {
        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Monkey)
            .StartAsync("Thinking...", async _ => await action());
    }

    public void DisplayError(Exception ex)
    {
        AnsiConsole.WriteException(ex, ExceptionFormats.ShortenEverything);
    }

    public async Task<IReadOnlyList<ChatMessage>> DisplayStreamingUpdatesAsync(
       IAsyncEnumerable<ChatResponseUpdate> updates)
    {
        var messageUpdates = new List<ChatResponseUpdate>();
        var paragraph = new Paragraph(string.Empty);

        var (headerText, justify, style) = GetUserStyle(ChatRole.Assistant);
        var header = new PanelHeader(headerText, justify);

        var panel = new Panel(paragraph)
            .RoundedBorder()
            .BorderStyle(style)
            .Header(header)
            .Expand();

        var callNames = new Dictionary<string, string>();

        await AnsiConsole.Live(panel)
            .AutoClear(false)
            .StartAsync(async ctx =>
            {
                await foreach (var update in updates)
                {
                    messageUpdates.Add(update);

                    var contents = update.Contents ?? Array.Empty<AIContent>();

                    if (update.Role == ChatRole.Assistant)
                    {
                        foreach (var call in contents.OfType<FunctionCallContent>())
                        {
                            if (!string.IsNullOrEmpty(call.CallId) && !string.IsNullOrEmpty(call.Name))
                            {
                                callNames[call.CallId] = call.Name;
                            }
                        }

                        _ = paragraph.Append(update.Text.EscapeMarkup());
                    }

                    foreach (var result in contents.OfType<FunctionResultContent>())
                    {
                        _ = paragraph.Append("\n");
                        string toolName = update.AuthorName ?? update.Role.GetValueOrDefault().ToString();
                        var id = result.CallId;
                        if (!string.IsNullOrEmpty(id) && callNames.TryGetValue(id, out var nameFound))
                        {
                            toolName = nameFound;
                        }

                        _ = paragraph.Append($"[grey]:wrench: {toolName} Result...[/]");
                    }

                    ctx.Refresh();
                }
            });

        var response = Microsoft.Extensions.AI.ChatResponseExtensions.ToChatResponse(messageUpdates);
        return [.. response.Messages];
    }
}
