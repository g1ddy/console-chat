using System.Collections.Generic;
using System.Linq;
using RadLine;

namespace SemanticKernelChat.Console;

public sealed class CommandCompletion : ITextCompletion
{
    private readonly List<IChatCommandStrategy> _commands;

    public CommandCompletion(IEnumerable<IChatCommandStrategy> commands)
    {
        _commands = commands.ToList();
    }

    public IEnumerable<string>? GetCompletions(string prefix, string word, string suffix)
    {
        var results = new List<string>();

        foreach (var cmd in _commands)
        {
            var comps = cmd.GetCompletions(prefix, word, suffix);
            if (comps is not null)
            {
                results.AddRange(comps);
            }
        }

        return results.Count > 0 ? results.Distinct(StringComparer.OrdinalIgnoreCase) : null;
    }
}
