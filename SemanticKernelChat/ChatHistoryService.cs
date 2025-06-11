using Microsoft.Extensions.AI;

namespace SemanticKernelChat;

public interface IChatHistoryService
{
    IReadOnlyList<ChatMessage> Messages { get; }
    void Add(ChatMessage message);
    void AddUserMessage(string text);
    void AddAssistantMessage(string text);
}

public class ChatHistoryService : IChatHistoryService
{
    private readonly List<ChatMessage> _messages = [];

    public IReadOnlyList<ChatMessage> Messages => _messages;

    public void Add(ChatMessage message)
    {
        if (!string.IsNullOrWhiteSpace(message.Text))
        {
            _messages.Add(message);
        }
    }

    public void AddUserMessage(string text) => Add(new ChatMessage(ChatRole.User, text));

    public void AddAssistantMessage(string text) => Add(new ChatMessage(ChatRole.Assistant, text));
}
