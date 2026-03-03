using RadLine;
using Spectre.Console;

namespace SemanticKernelChat.Console;

public interface IChatLineEditor
{
    Task<string?> ReadLine(CancellationToken cancellationToken);
}

public sealed class ChatLineEditor : IChatLineEditor
{
    private const string HistoryEnvVar = "CHAT_HISTORY_FILE";

    private readonly LineEditor _editor;
    private readonly string? _historyPath;

    public ChatLineEditor(ITextCompletion completion, IAnsiConsole console)
    {
        _editor = new LineEditor
        {
            MultiLine = true,
            Completion = completion,
        };

        // Use Up/Down arrows for history navigation
        _editor.KeyBindings.Remove(ConsoleKey.UpArrow);
        _editor.KeyBindings.Remove(ConsoleKey.DownArrow);
        _editor.KeyBindings.Add<PreviousHistoryCommand>(ConsoleKey.UpArrow);
        _editor.KeyBindings.Add<NextHistoryCommand>(ConsoleKey.DownArrow);

        var rawPath = Environment.GetEnvironmentVariable(HistoryEnvVar);
        if (!string.IsNullOrEmpty(rawPath))
        {
            if (IsPathSafe(rawPath, out var validatedPath))
            {
                _historyPath = validatedPath;
                if (File.Exists(_historyPath))
                {
                    try
                    {
                        foreach (var line in File.ReadLines(_historyPath))
                        {
                            if (!string.IsNullOrWhiteSpace(line))
                            {
                                _editor.History.Add(line);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        console.MarkupLine($"[yellow]Warning: Failed to load chat history from '{Markup.Escape(_historyPath)}'. {Markup.Escape(ex.Message)}[/]");
                    }
                }
            }
            else
            {
                console.MarkupLine($"[red]Warning: Chat history file path '{Markup.Escape(rawPath)}' is outside the safe directory and will be ignored.[/]");
            }
        }
    }

    private static bool IsPathSafe(string path, out string? validatedPath)
    {
        validatedPath = null;
        try
        {
            var fullPath = Path.GetFullPath(path);
            var safeRoot = Path.GetFullPath(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));

            // Ensure safeRoot ends with a directory separator for accurate prefix matching
            var safeRootWithSeparator = safeRoot.EndsWith(Path.DirectorySeparatorChar)
                ? safeRoot
                : safeRoot + Path.DirectorySeparatorChar;

            // Allow files only within the safeRoot
            if (fullPath.StartsWith(safeRootWithSeparator, StringComparison.OrdinalIgnoreCase) ||
                fullPath.Equals(safeRoot, StringComparison.OrdinalIgnoreCase))
            {
                validatedPath = fullPath;
                return true;
            }
        }
        catch
        {
            // Fall through to return false
        }

        return false;
    }

    public async Task<string?> ReadLine(CancellationToken cancellationToken)
    {
        var line = await _editor.ReadLine(cancellationToken);
        if (!string.IsNullOrWhiteSpace(line))
        {
            _editor.History.Add(line);

            if (!string.IsNullOrEmpty(_historyPath))
            {
                try
                {
                    await File.AppendAllTextAsync(_historyPath, line + Environment.NewLine, cancellationToken);
                }
                catch
                {
                    // Silent failure on write to avoid interrupting the chat flow
                }
            }
        }

        return line;
    }
}
