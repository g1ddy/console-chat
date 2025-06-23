using SemanticKernelChat.Infrastructure;
using System.Collections.Generic;
using System.Linq;

namespace SemanticKernelChat.Console;

public sealed class SetMcpServerStateCommandStrategy : IChatCommandStrategy
{
    private readonly McpToolCollection _tools;

    public SetMcpServerStateCommandStrategy(McpToolCollection tools)
    {
        _tools = tools;
    }

    public IEnumerable<string>? GetCompletions(string prefix, string word, string suffix)
    {
        var tokens = (prefix + word).TrimStart().Split(' ', StringSplitOptions.TrimEntries);
        if (tokens.Length == 1)
        {
            return new[] { CliConstants.Commands.Enable, CliConstants.Commands.Disable }
                .Where(c => c.StartsWith(word, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }
        if (tokens.Length == 2 &&
            (tokens[0].Equals(CliConstants.Commands.Enable, StringComparison.OrdinalIgnoreCase) ||
             tokens[0].Equals(CliConstants.Commands.Disable, StringComparison.OrdinalIgnoreCase)))
        {
            return CliConstants.Options.Mcp.StartsWith(word, StringComparison.OrdinalIgnoreCase)
                ? new[] { CliConstants.Options.Mcp }
                : Array.Empty<string>();
        }
        if (tokens.Length >= 3 &&
            (tokens[0].Equals(CliConstants.Commands.Enable, StringComparison.OrdinalIgnoreCase) ||
             tokens[0].Equals(CliConstants.Commands.Disable, StringComparison.OrdinalIgnoreCase)) &&
            tokens[1].Equals(CliConstants.Options.Mcp, StringComparison.OrdinalIgnoreCase))
        {
            return _tools.Servers.Where(n => n.StartsWith(word, StringComparison.OrdinalIgnoreCase)).ToList();
        }
        return null;
    }

    public bool CanExecute(string input)
    {
        var tokens = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (tokens.Length != 3)
        {
            return false;
        }

        bool hasCommand = tokens[0].Equals(CliConstants.Commands.Enable, StringComparison.OrdinalIgnoreCase) ||
                           tokens[0].Equals(CliConstants.Commands.Disable, StringComparison.OrdinalIgnoreCase);

        return hasCommand &&
               tokens[1].Equals(CliConstants.Options.Mcp, StringComparison.OrdinalIgnoreCase) &&
               _tools.Servers.Contains(tokens[2]);
    }

    public Task<bool> ExecuteAsync(string input, IChatHistoryService history, IChatController controller, IChatConsole console)
    {
        var tokens = input.Split(' ', StringSplitOptions.TrimEntries);
        if (tokens.Length < 3)
        {
            return Task.FromResult(true);
        }

        bool enable = tokens[0].Equals(CliConstants.Commands.Enable, StringComparison.OrdinalIgnoreCase);
        string name = tokens[2];
        _tools.SetServerEnabled(name, enable);
        return Task.FromResult(true);
    }
}
