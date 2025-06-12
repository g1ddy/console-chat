using Microsoft.Extensions.AI;

namespace SemanticKernelChat;

public interface IChatHistoryService
{
    IReadOnlyList<ChatMessage> Messages { get; }
    void Add(params ChatMessage[] messages);
    void Add(ChatResponseUpdate[] messageUpdates);
    void AddUserMessage(string text);
    void AddAssistantMessage(string text);
}

public class ChatHistoryService : IChatHistoryService
{
    private readonly List<ChatMessage> _messages = [];

    public IReadOnlyList<ChatMessage> Messages => _messages;

    public void Add(params ChatMessage[] messages) => _messages.AddRange(messages);

    public void Add(ChatResponseUpdate[] messageUpdates) => _messages.AddMessages(messageUpdates);

    public void AddUserMessage(string text) => Add(new ChatMessage(ChatRole.User, text));

    public void AddAssistantMessage(string text) => Add(new ChatMessage(ChatRole.Assistant, text));
}
