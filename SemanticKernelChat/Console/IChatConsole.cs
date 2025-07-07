using Microsoft.Extensions.AI;
using Spectre.Console.Rendering;

namespace SemanticKernelChat.Console;

public interface IChatConsole
{
    void WriteChatMessages(params ChatMessage[] messages);
    void WriteWelcomeMessage();
    void WriteExitMessage();
    void WriteHeader(ChatRole role);
    void WriteUserPrompt();
    void WriteLine(string text);
    void Write(IRenderable renderable);
    Task<string?> ReadMultilineInputAsync(CancellationToken cancellationToken = default);
    Task DisplayThinkingIndicator(Func<Task> action);
    void DisplayError(Exception ex);
    Task<IReadOnlyList<ChatMessage>> DisplayStreamingUpdatesAsync(IAsyncEnumerable<ChatResponseUpdate> updates);
    IReadOnlyList<string> PromptMultiSelection(string title, IEnumerable<(string Name, bool Selected)> items);

    bool DebugEnabled { get; set; }
}
