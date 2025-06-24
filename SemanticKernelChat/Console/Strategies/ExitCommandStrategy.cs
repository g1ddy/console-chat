using System.Collections.Generic;

namespace SemanticKernelChat.Console;

public sealed class ExitCommandStrategy : IChatCommandStrategy
{
    public IEnumerable<string>? GetCompletions(string prefix, string word, string suffix)
    {
        var input = (prefix + word).TrimStart();
        return CliConstants.Commands.Exit.StartsWith(input, StringComparison.OrdinalIgnoreCase)
            ? new[] { CliConstants.Commands.Exit }
            : null;
    }

    public bool CanExecute(string input) =>
        input.Equals(CliConstants.Commands.Exit, StringComparison.OrdinalIgnoreCase);

    public Task<bool> ExecuteAsync(string input, IChatHistoryService history, IChatController controller, IChatConsole console)
    {
        console.WriteExitMessage();
        return Task.FromResult(false); // stop loop
    }
}
