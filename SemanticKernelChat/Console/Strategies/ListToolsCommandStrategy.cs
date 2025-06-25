using SemanticKernelChat.Infrastructure;
using Spectre.Console;
using Spectre.Console.Rendering;
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
        var infos = _tools.GetServerInfos();
        var enabled = infos.Where(i => i.Enabled).ToList();
        var disabled = infos.Where(i => !i.Enabled).ToList();

        var columnContent = new List<IRenderable>();

        foreach (var info in enabled)
        {
            var tree = new Tree($"{info.Name} ({info.Status})");
            foreach (var tool in info.Tools)
            {
                tree.AddNode(tool.Name);
            }
            columnContent.Add(tree);
        }

        if (disabled.Count > 0)
        {
            var tree = new Tree("Disabled");
            foreach (var info in disabled)
            {
                tree.AddNode($"{info.Name} ({info.Status})");
            }
            columnContent.Add(tree);
        }
        var columns = new Columns(columnContent) { Expand = true };
        AnsiConsole.Write(columns);
        return Task.FromResult(true);
    }
}

