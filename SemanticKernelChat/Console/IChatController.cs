using Microsoft.Extensions.AI;
using SemanticKernelChat;
using SemanticKernelChat.Infrastructure;

namespace SemanticKernelChat.Console;

public interface IChatController
{
    Task SendAndDisplayAsync(IChatHistoryService history);
    Task SendAndDisplayStreamingAsync(IChatHistoryService history, Action<IReadOnlyList<ChatMessage>>? finalCallback = null);
    Task SummarizeAsync(IChatHistoryService history);
    McpToolCollection ToolCollection { get; }
}
