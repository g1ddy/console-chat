using System.IO;
using RadLine;

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

    public ChatLineEditor(ITextCompletion completion)
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

        _historyPath = Environment.GetEnvironmentVariable(HistoryEnvVar);
        if (!string.IsNullOrEmpty(_historyPath) && File.Exists(_historyPath))
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
                System.Console.Error.WriteLine($"Warning: Failed to load chat history from '{_historyPath}'. {ex.Message}");
            }
        }
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
                catch (Exception ex)
                {
                    System.Console.Error.WriteLine($"Warning: Failed to write to history file: {ex.Message}");
                }
            }
        }

        return line;
    }
}
