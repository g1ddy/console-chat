using Microsoft.Extensions.AI;
using SemanticKernelChat;

namespace SemanticKernelChat.Console;

public interface IChatController
{
    Task SendAndDisplayAsync(IChatHistoryService history);
    Task SendAndDisplayStreamingAsync(IChatHistoryService history, Action<IReadOnlyList<ChatMessage>>? finalCallback = null);
}
