using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;

namespace SemanticKernelChat.Console;

public interface IChatConsole
{
    void Initialize(IEnumerable<string> pluginNames);
    void WriteChatMessages(params ChatMessage[] messages);
    Task<string?> ReadMultilineInputAsync();
    Task DisplayThinkingIndicator(Func<Task> action);
    void DisplayError(Exception ex);
    Task<IReadOnlyList<ChatMessage>> DisplayStreamingUpdatesAsync(IAsyncEnumerable<ChatResponseUpdate> updates);
}
