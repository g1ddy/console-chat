using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SemanticKernelChat.Console;

public sealed class SummarizeHistoryCommandStrategy : IChatCommandStrategy
{
    public IEnumerable<string>? GetCompletions(string prefix, string word, string suffix)
    {
        var tokens = (prefix + word).TrimStart().Split(' ', StringSplitOptions.TrimEntries);
        if (tokens.Length == 1)
        {
            return new[] { CliConstants.Commands.Summarize };
        }
        return null;
    }

    public bool CanExecute(string input)
    {
        return string.Equals(input.Trim(), CliConstants.Commands.Summarize, StringComparison.OrdinalIgnoreCase);
    }

    public async Task<bool> ExecuteAsync(string input, IChatHistoryService history, IChatController controller, IChatConsole console)
    {
        await controller.SummarizeAsync(history);
        return true;
    }
}
