using SemanticKernelChat.Infrastructure;
using System.Collections.Generic;
using System.Linq;

namespace SemanticKernelChat.Console;

public sealed class ListToolsCommandStrategy : IChatCommandStrategy
{
    public const string Command = CliConstants.Commands.List;
    private readonly McpToolCollection _tools;

    public ListToolsCommandStrategy(McpToolCollection tools)
    {
        _tools = tools;
    }

    public IEnumerable<string>? GetCompletions(string prefix, string word, string suffix)
    {
        var tokens = (prefix + word).TrimStart().Split(' ', StringSplitOptions.TrimEntries);
        if (tokens.Length == 1)
        {
            return new[] { CliConstants.Commands.List };
        }
        if (tokens.Length == 2 && tokens[0].Equals(CliConstants.Commands.List, StringComparison.OrdinalIgnoreCase))
        {
            return new[] { CliConstants.Options.Tools };
        }
        return null;
    }

    public bool CanExecute(string input)
    {
        var tokens = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return tokens.Length == 2 &&
               tokens[0].Equals(CliConstants.Commands.List, StringComparison.OrdinalIgnoreCase) &&
               tokens[1].Equals(CliConstants.Options.Tools, StringComparison.OrdinalIgnoreCase);
    }

    public Task<bool> ExecuteAsync(string input, IChatHistoryService history, IChatController controller, IChatConsole console)
    {
        var infos = controller.ToolCollection.GetServerInfos();
        foreach (var info in infos)
        {
            console.WriteLine($"{info.Name} ({(info.Enabled ? "enabled" : "disabled")}, {info.Status})");
            if (info.Enabled && info.Status == ServerStatus.Ready)
            {
                foreach (var tool in info.Tools)
                {
                    console.WriteLine($"  - {tool.Name}");
                }
            }
        }
        return Task.FromResult(true);
    }
}

