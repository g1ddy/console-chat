using SemanticKernelChat.Infrastructure;
using System.Collections.Generic;
using System.Linq;

namespace SemanticKernelChat.Console;

public sealed class ToggleMcpServerCommandStrategy : IChatCommandStrategy
{
    private readonly McpToolCollection _tools;

    public ToggleMcpServerCommandStrategy(McpToolCollection tools)
    {
        _tools = tools;
    }

    public IEnumerable<string>? GetCompletions(string prefix, string word, string suffix)
    {
        var tokens = string.Concat(prefix, word).TrimStart().SplitCommandArguments();
        if (tokens.Length == 1)
        {
            return new[] { CliConstants.Commands.Toggle };
        }
        if (tokens.Length == 2 && tokens[0].Equals(CliConstants.Commands.Toggle, StringComparison.OrdinalIgnoreCase))
        {
            return new[] { CliConstants.Options.Mcp };
        }
        return null;
    }

    public bool CanExecute(string input)
    {
        var tokens = input.SplitCommandArguments();
        return tokens.Length == 2 &&
               tokens[0].Equals(CliConstants.Commands.Toggle, StringComparison.OrdinalIgnoreCase) &&
               tokens[1].Equals(CliConstants.Options.Mcp, StringComparison.OrdinalIgnoreCase);
    }

    public Task<bool> ExecuteAsync(string input, IChatHistoryService history, IChatController controller, IChatConsole console)
    {
        var choices = _tools.Servers.Select(n => (Name: n, Selected: _tools.IsServerEnabled(n)));
        var selected = console.PromptMultiSelection("Toggle MCP servers", choices).ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var name in _tools.Servers)
        {
            _tools.SetServerEnabled(name, selected.Contains(name));
        }
        return Task.FromResult(true);
    }
}
