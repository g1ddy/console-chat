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
    private const string SafeHistorySubdir = ".config/semantickernelchat";

    internal readonly LineEditor _editor;
    private readonly IAnsiConsole _console;
    private readonly string? _historyPath;
    internal readonly Lazy<Task> _historyLoader;

    public ChatLineEditor(ITextCompletion completion, IAnsiConsole console)
    {
        _console = console;
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
            var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile, Environment.SpecialFolderOption.DoNotVerify);
            var safeRoot = Path.GetFullPath(Path.Combine(userProfile, SafeHistorySubdir));

            if (IsPathSafe(rawPath, safeRoot, out var validatedPath))
            {
                _historyPath = validatedPath;
            }
            else
            {
                _console.MarkupLine($"[red]Warning: Chat history file path '{Markup.Escape(rawPath)}' is invalid or outside the safe directory and will be ignored.[/]");
            }
        }

        _historyLoader = new Lazy<Task>(LoadHistoryAsync);
    }

    private async Task LoadHistoryAsync()
    {
        if (string.IsNullOrEmpty(_historyPath) || !File.Exists(_historyPath))
        {
            return;
        }

        try
        {
            await foreach (var line in File.ReadLinesAsync(_historyPath))
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    _editor.History.Add(line);
                }
            }
        }
        catch (Exception ex)
        {
            _console.MarkupLine($"[yellow]Warning: Failed to load chat history from '{Markup.Escape(_historyPath)}'. {Markup.Escape(ex.Message)}[/]");
        }
    }

    internal static bool IsPathSafe(string path, string safeRoot, out string? validatedPath)
    {
        validatedPath = null;
        try
        {
            var fullPath = Path.GetFullPath(path);
            safeRoot = Path.GetFullPath(safeRoot);

            // Ensure the safe directory exists
            if (!Directory.Exists(safeRoot))
            {
                Directory.CreateDirectory(safeRoot);
            }

            // Ensure safeRoot ends with a directory separator for accurate prefix matching
            var safeRootWithSeparator = safeRoot.EndsWith(Path.DirectorySeparatorChar)
                ? safeRoot
                : safeRoot + Path.DirectorySeparatorChar;

            // Walk the path components to ensure none are reparse points or hidden
            var current = fullPath;
            while (!string.IsNullOrEmpty(current) && current.Length >= safeRoot.Length)
            {
                // Note: FileInfo works for both files and directories for these checks
                // On Unix, we use LinkTarget to check for symlinks because FileAttributes.ReparsePoint might not be set for all symlink types
                // or might require the file to exist.
                var info = new FileInfo(current);

                if (info.LinkTarget != null)
                {
                    return false;
                }

                // Reject if it's a reparse point (prevent link-following traversal)
                // We only allow reparse points if they are part of the safeRoot prefix itself (e.g., if the home dir is a symlink)
                // but for simplicity and maximum security, we'll check everything from fullPath up to the safeRoot
                if (info.Exists && (info.Attributes & FileAttributes.ReparsePoint) != 0)
                {
                    return false;
                }

                // Reject hidden files or directories (starting with a dot)
                // Exception: The safe directory itself starts with a dot, so we stop before checking it.
                if (current.Length > safeRoot.Length)
                {
                    var name = Path.GetFileName(current);
                    if (name.StartsWith('.'))
                    {
                        return false;
                    }
                }

                current = Path.GetDirectoryName(current);
            }

            // Allow files only within the safeRoot
            if (fullPath.StartsWith(safeRootWithSeparator, StringComparison.OrdinalIgnoreCase))
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
        await _historyLoader.Value;

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
                catch (Exception ex)
                {
                    _console.MarkupLine($"[yellow]Warning: Failed to write to history file: {Markup.Escape(ex.Message)}[/]");
                }
            }
        }

        return line;
    }
}
