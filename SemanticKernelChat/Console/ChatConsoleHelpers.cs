using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.AI;
using Spectre.Console;
using Spectre.Console.Json;
using Spectre.Console.Rendering;

namespace SemanticKernelChat.Console;

internal static class ChatConsoleHelpers
{
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

    public static void CollectFunctionCallNames(IEnumerable<AIContent> contents, Dictionary<string, string> callNames)
    {
        foreach (var call in contents.OfType<FunctionCallContent>())
        {
            if (!string.IsNullOrEmpty(call.CallId) && !string.IsNullOrEmpty(call.Name))
            {
                callNames[call.CallId] = call.Name;
            }
        }
    }

    public static Dictionary<string, string> CollectFunctionCallNames(IEnumerable<ChatMessage> messages)
    {
        var callNames = new Dictionary<string, string>();
        foreach (var message in messages)
        {
            CollectFunctionCallNames(message.Contents, callNames);
        }

        return callNames;
    }

    public static string GetToolName(
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

    public static bool IsValidJson(string text)
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

    public static string? SerializeArguments(IDictionary<string, object?>? args)
        => args?.Count > 0 ? JsonSerializer.Serialize(args) : null;

    public static IEnumerable<IRenderable> FormatPanelContent(string markup, string? rawJson, bool debugEnabled)
    {
        var rows = new List<IRenderable> { new Markup(markup) };

        if (debugEnabled && !string.IsNullOrWhiteSpace(rawJson))
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

    public static Panel CreatePanel(IRenderable content, Style style, PanelHeader header)
        => new Panel(content)
            .RoundedBorder()
            .BorderStyle(style)
            .Header(header)
            .Expand();
}
