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
        var tokens = string.Concat(prefix, word).TrimStart().SplitCommandArguments();
        if (tokens.Length == 1)
        {
            return new[] { CliConstants.Commands.Enable, CliConstants.Commands.Disable };
        }
        if (tokens.Length == 2 && IsEnableOrDisable(tokens[0]))
        {
            return new[] { CliConstants.Options.Mcp };
        }
        if (tokens.Length >= 3 && IsEnableOrDisable(tokens[0]) && IsMcpOption(tokens[1]))
        {
            return _tools.Servers.ToList();
        }
        return null;
    }

    public bool CanExecute(string input)
    {
        var tokens = input.SplitCommandArguments();
        if (tokens.Length != 3)
        {
            return false;
        }

        return IsEnableOrDisable(tokens[0]) &&
               IsMcpOption(tokens[1]) &&
               _tools.Servers.Contains(tokens[2]);
    }

    public Task<bool> ExecuteAsync(string input, IChatHistoryService history, IChatController controller, IChatConsole console)
    {
        var tokens = input.SplitCommandArguments();
        if (tokens.Length < 3)
        {
            return Task.FromResult(true);
        }

        bool enable = tokens[0].Equals(CliConstants.Commands.Enable, StringComparison.OrdinalIgnoreCase);
        string name = tokens[2];
        _tools.SetServerEnabled(name, enable);
        return Task.FromResult(true);
    }

    private static bool IsEnableOrDisable(string token)
    {
        return token.Equals(CliConstants.Commands.Enable, StringComparison.OrdinalIgnoreCase) ||
               token.Equals(CliConstants.Commands.Disable, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsMcpOption(string token)
    {
        return token.Equals(CliConstants.Options.Mcp, StringComparison.OrdinalIgnoreCase);
    }
}
