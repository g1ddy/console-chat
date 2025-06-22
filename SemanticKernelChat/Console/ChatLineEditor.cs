using RadLine;

using SemanticKernelChat.Infrastructure;

namespace SemanticKernelChat.Console;

public interface IChatLineEditor
{
    Task<string?> ReadLine(CancellationToken cancellationToken);
}

public sealed class ChatLineEditor : IChatLineEditor
{
    private LineEditor _editor = default!;

    public ChatLineEditor(McpToolCollection tools)
    {
        var pluginNames = tools.Plugins.Keys;
        ConfigureCompletion(pluginNames);
    }

    public Task<string?> ReadLine(CancellationToken cancellationToken)
        => _editor.ReadLine(cancellationToken);

    private void ConfigureCompletion(IEnumerable<string> pluginNames)
    {
        var completion = new CommandCompletion(pluginNames);
        _editor = new LineEditor
        {
            MultiLine = true,
            Completion = completion,
        };
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
            if (cmd.Equals(CliConstants.Commands.Enable, StringComparison.OrdinalIgnoreCase) ||
                cmd.Equals(CliConstants.Commands.Disable, StringComparison.OrdinalIgnoreCase))
            {
                if (tokens.Length == 2)
                {
                    var part = tokens[1];
                    var option = CliConstants.Options.Mcp;
                    return option.StartsWith(part, StringComparison.OrdinalIgnoreCase)
                        ? new[] { option }
                        : Array.Empty<string>();
                }
                else if (tokens.Length == 3 && tokens[1].Equals(CliConstants.Options.Mcp, StringComparison.OrdinalIgnoreCase))
                {
                    var part = tokens[2];
                    return _toolNames.Where(t => t.StartsWith(part, StringComparison.OrdinalIgnoreCase));
                }
            }

            if (cmd.Equals(CliConstants.Commands.Toggle, StringComparison.OrdinalIgnoreCase))
            {
                if (tokens.Length == 2)
                {
                    var part = tokens[1];
                    var option = CliConstants.Options.Mcp;
                    return option.StartsWith(part, StringComparison.OrdinalIgnoreCase)
                        ? new[] { option }
                        : Array.Empty<string>();
                }
                else if (tokens.Length == 3 && tokens[1].Equals(CliConstants.Options.Mcp, StringComparison.OrdinalIgnoreCase))
                {
                    var part = tokens[2];
                    return _toolNames.Where(t => t.StartsWith(part, StringComparison.OrdinalIgnoreCase));
                }
            }

            return null;
        }
    }
}
