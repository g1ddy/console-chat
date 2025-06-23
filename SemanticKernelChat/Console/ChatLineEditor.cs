using RadLine;

using SemanticKernelChat.Infrastructure;
using SemanticKernelChat.Console;
using System.Collections.Generic;
using System.Linq;

namespace SemanticKernelChat.Console;

public interface IChatLineEditor
{
    Task<string?> ReadLine(CancellationToken cancellationToken);
}

public sealed class ChatLineEditor : IChatLineEditor
{
    private LineEditor _editor = default!;
    private readonly List<IChatCommandStrategy> _commands;

    public ChatLineEditor(McpToolCollection tools, IEnumerable<IChatCommandStrategy> commands)
    {
        _commands = commands.ToList();
        var pluginNames = tools.Plugins.Keys;
        ConfigureCompletion(pluginNames);
    }

    public Task<string?> ReadLine(CancellationToken cancellationToken)
        => _editor.ReadLine(cancellationToken);

    private void ConfigureCompletion(IEnumerable<string> pluginNames)
    {
        var completion = new CommandCompletion(pluginNames, _commands);
        _editor = new LineEditor
        {
            MultiLine = true,
            Completion = completion,
        };
    }

    private sealed class CommandCompletion : ITextCompletion
    {
        private readonly List<string> _toolNames;
        private readonly List<IChatCommandStrategy> _commands;

        public CommandCompletion(IEnumerable<string> toolNames, IEnumerable<IChatCommandStrategy> commands)
        {
            _toolNames = toolNames.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            _commands = commands.ToList();
        }

        public IEnumerable<string>? GetCompletions(string prefix, string word, string suffix)
        {
            var tokens = (prefix + word).TrimStart().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var results = new List<string>();

            foreach (var cmd in _commands)
            {
                var comps = cmd.GetCompletions(prefix, word, suffix);
                if (comps is not null)
                    results.AddRange(comps);
            }

            if (tokens.Length <= 1)
            {
                results.AddRange(_toolNames.Where(t => t.StartsWith(word, StringComparison.OrdinalIgnoreCase)));
            }

            return results.Count > 0 ? results.Distinct(StringComparer.OrdinalIgnoreCase) : null;
        }
    }
}
