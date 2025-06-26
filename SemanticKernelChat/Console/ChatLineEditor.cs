using RadLine;

namespace SemanticKernelChat.Console;

public interface IChatLineEditor
{
    Task<string?> ReadLine(CancellationToken cancellationToken);
}

public sealed class ChatLineEditor : IChatLineEditor
{
    private readonly LineEditor _editor;

    public ChatLineEditor(ITextCompletion completion)
    {
        _editor = new LineEditor
        {
            MultiLine = true,
            Completion = completion,
        };
    }

    public Task<string?> ReadLine(CancellationToken cancellationToken)
        => _editor.ReadLine(cancellationToken);
}
