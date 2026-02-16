using System.Collections.Generic;

namespace SemanticKernelChat.Console;

public sealed class DebugCommandStrategy : IChatCommandStrategy
{
    public IEnumerable<string>? GetCompletions(string prefix, string word, string suffix)
    {
        var tokens = string.Concat(prefix, word).TrimStart().Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (tokens.Length == 1)
        {
            return new[] { CliConstants.Commands.Debug };
        }
        if (tokens.Length == 2 && tokens[0].Equals(CliConstants.Commands.Debug, StringComparison.OrdinalIgnoreCase))
        {
            return new[] { "on", "off" };
        }
        return null;
    }

    public bool CanExecute(string input)
    {
        var tokens = (input ?? string.Empty).Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (tokens.Length == 1)
        {
            return tokens[0].Equals(CliConstants.Commands.Debug, StringComparison.OrdinalIgnoreCase);
        }
        if (tokens.Length == 2)
        {
            return tokens[0].Equals(CliConstants.Commands.Debug, StringComparison.OrdinalIgnoreCase) &&
                   (tokens[1].Equals("on", StringComparison.OrdinalIgnoreCase) || tokens[1].Equals("off", StringComparison.OrdinalIgnoreCase));
        }
        return false;
    }

    public Task<bool> ExecuteAsync(string input, IChatHistoryService history, IChatController controller, IChatConsole console)
    {
        var tokens = (input ?? string.Empty).Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (tokens.Length == 1)
        {
            console.DebugEnabled = !console.DebugEnabled;
        }
        else if (tokens.Length >= 2)
        {
            console.DebugEnabled = tokens[1].Equals("on", StringComparison.OrdinalIgnoreCase);
        }

        console.WriteLine($"Debug {(console.DebugEnabled ? "enabled" : "disabled")}");
        return Task.FromResult(true);
    }
}
