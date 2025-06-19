using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using RadLine;

namespace SemanticKernelChat.Console;

public interface IChatLineEditor
{
    Task<string?> ReadLine(CancellationToken cancellationToken);
    void ConfigureCompletion(IEnumerable<string>? pluginNames = null);
}

public sealed class ChatLineEditor : IChatLineEditor
{
    private readonly LineEditor _editor;
    private readonly Kernel _kernel;

    public ChatLineEditor(Kernel kernel)
    {
        _kernel = kernel;
        _editor = new LineEditor { MultiLine = true };
    }

    public Task<string?> ReadLine(CancellationToken cancellationToken) =>
        _editor.ReadLine(cancellationToken);

    public void ConfigureCompletion(IEnumerable<string>? pluginNames = null)
    {
        pluginNames ??= _kernel.Plugins.Select(p => p.Name);
        var completion = new CommandCompletion(pluginNames);
        typeof(LineEditor)
            .GetProperty(nameof(LineEditor.Completion))!
            .SetValue(_editor, completion);
    }

    private sealed class CommandCompletion : ITextCompletion
    {
        private readonly List<string> _toolNames;

        public CommandCompletion(IEnumerable<string> toolNames)
        {
            _toolNames = toolNames.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        }

        public IEnumerable<string>? GetCompletions(string prefix, string word, string suffix)
        {
            var tokens = (prefix + word).TrimStart().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length <= 1)
            {
                return CliConstants.Commands.All.Concat(_toolNames).Where(c => c.StartsWith(word, StringComparison.OrdinalIgnoreCase));
            }

            var cmd = tokens[0];
            if ((cmd.Equals(CliConstants.Commands.Enable, StringComparison.OrdinalIgnoreCase) ||
                cmd.Equals(CliConstants.Commands.Disable, StringComparison.OrdinalIgnoreCase) ||
                cmd.Equals(CliConstants.Commands.Toggle, StringComparison.OrdinalIgnoreCase)) &&
                tokens.Length == 2)
            {
                var part = tokens[1];
                return _toolNames.Where(t => t.StartsWith(part, StringComparison.OrdinalIgnoreCase));
            }

            return null;
        }
    }
}
