using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;

namespace SemanticKernelChat.Console;

public interface IChatConsole
{
    void WriteChatMessages(params ChatMessage[] messages);
    void WriteWelcomeMessage();
    void WriteExitMessage();
    void WriteHeader(ChatRole role);
    void WriteUserPrompt();
    void WriteLine(string text);
    Task<string?> ReadMultilineInputAsync();
    Task DisplayThinkingIndicator(Func<Task> action);
    void DisplayError(Exception ex);
    Task<IReadOnlyList<ChatMessage>> DisplayStreamingUpdatesAsync(IAsyncEnumerable<ChatResponseUpdate> updates);
}
