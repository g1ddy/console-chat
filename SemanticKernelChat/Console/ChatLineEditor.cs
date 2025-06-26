using RadLine;

using System.Collections.Generic;
using System.Linq;

namespace SemanticKernelChat.Console;

public interface IChatLineEditor
{
    Task<string?> ReadLine(CancellationToken cancellationToken);
}

public sealed class ChatLineEditor : IChatLineEditor
{
    private readonly LineEditor _editor;
    private readonly List<IChatCommandStrategy> _commands;

    public ChatLineEditor(IEnumerable<IChatCommandStrategy> commands)
    {
        _commands = commands.ToList();
        _editor = CreateEditor();
    }

    public Task<string?> ReadLine(CancellationToken cancellationToken)
        => _editor.ReadLine(cancellationToken);

    private LineEditor CreateEditor()
    {
        var completion = new CommandCompletion(_commands);
        return new LineEditor
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
