namespace SemanticKernelChat;

public interface IChatHistoryReducer
{
    Task<IEnumerable<Microsoft.Extensions.AI.ChatMessage>?> ReduceAsync(
        IReadOnlyList<Microsoft.Extensions.AI.ChatMessage> chatHistory,
        CancellationToken cancellationToken = default);
}
