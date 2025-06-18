using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;

namespace SemanticKernelChat.Console;

public interface IChatController
{
    Task SendAndDisplayAsync(IChatClient chatClient, IChatHistoryService history, IReadOnlyList<McpClientTool> tools);
    Task SendAndDisplayStreamingAsync(IChatClient chatClient, IChatHistoryService history, IReadOnlyList<McpClientTool> tools, Action<IReadOnlyList<ChatMessage>>? finalCallback = null);
}
