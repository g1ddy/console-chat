using SemanticKernelChat.Infrastructure;
using Spectre.Console;
using Spectre.Console.Rendering;
using System.Collections.Generic;
using System.Linq;

namespace SemanticKernelChat.Console;

internal abstract class ListCommandStrategyBase<TInfo> : IChatCommandStrategy
{
    private readonly string _optionName;

    protected ListCommandStrategyBase(string optionName)
    {
        _optionName = optionName;
    }

    protected abstract IEnumerable<TInfo> GetServerInfos();
    protected abstract bool IsEnabled(TInfo info);
    protected abstract string GetName(TInfo info);
    protected abstract ServerStatus GetStatus(TInfo info);
    protected abstract IEnumerable<string> GetItemNames(TInfo info);

    public IEnumerable<string>? GetCompletions(string prefix, string word, string suffix)
    {
        var tokens = CommandTokenizer.SplitArguments((prefix + word).TrimStart());
        if (tokens.Length == 1)
        {
            return new[] { CliConstants.Commands.List };
        }
        if (tokens.Length == 2 && tokens[0].Equals(CliConstants.Commands.List, StringComparison.OrdinalIgnoreCase))
        {
            return new[] { _optionName };
        }
        return null;
    }

    public bool CanExecute(string input)
    {
        var tokens = CommandTokenizer.SplitArguments(input);
        return tokens.Length == 2 &&
               tokens[0].Equals(CliConstants.Commands.List, StringComparison.OrdinalIgnoreCase) &&
               tokens[1].Equals(_optionName, StringComparison.OrdinalIgnoreCase);
    }

    public Task<bool> ExecuteAsync(string input, IChatHistoryService history, IChatController controller, IChatConsole console)
    {
        var infosLookup = GetServerInfos().ToLookup(IsEnabled);
        var enabled = infosLookup[true].ToList();
        var disabled = infosLookup[false].ToList();

        var columnContent = new List<IRenderable>();

        foreach (var info in enabled)
        {
            var tree = new Tree($"{GetName(info)} ({GetStatus(info)})");
            foreach (var name in GetItemNames(info))
            {
                tree.AddNode(name);
            }
            columnContent.Add(tree);
        }

        if (disabled.Count > 0)
        {
            var tree = new Tree("Disabled");
            foreach (var info in disabled)
            {
                tree.AddNode($"{GetName(info)} ({GetStatus(info)})");
            }
            columnContent.Add(tree);
        }
        var columns = new Columns(columnContent) { Expand = true };
        console.Write(columns);
        return Task.FromResult(true);
    }
}
