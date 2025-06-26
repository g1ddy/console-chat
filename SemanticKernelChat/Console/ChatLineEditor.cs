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
    private LineEditor? _editor;
    private readonly List<IChatCommandStrategy> _commands;

    public ChatLineEditor(McpToolCollection tools, IEnumerable<IChatCommandStrategy> commands)
    {
        _commands = commands.ToList();
        ConfigureCompletion();
    }

    public Task<string?> ReadLine(CancellationToken cancellationToken)
        => _editor is not null
            ? _editor.ReadLine(cancellationToken)
            : Task.FromResult<string?>(null);

    private void ConfigureCompletion()
    {
        var completion = new CommandCompletion(_commands);
        _editor = new LineEditor
        {
            MultiLine = true,
            Completion = completion,
        };
    }

    private sealed class CommandCompletion : ITextCompletion
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
                    results.AddRange(comps);
            }

            return results.Count > 0 ? results.Distinct(StringComparer.OrdinalIgnoreCase) : null;
        }
    }
}
