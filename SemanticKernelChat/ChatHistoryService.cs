using Microsoft.Extensions.AI;

namespace SemanticKernelChat;

public interface IChatHistoryService
{
    IReadOnlyList<ChatMessage> Messages { get; }
    void AddUserMessage(string text);
    void AddAssistantMessage(string text);
}

public class ChatHistoryService : IChatHistoryService
{
    private readonly List<ChatMessage> _messages = [];

    public IReadOnlyList<ChatMessage> Messages => _messages;

    public void AddUserMessage(string text)
    {
        if (!string.IsNullOrWhiteSpace(text))
        {
            _messages.Add(new ChatMessage(ChatRole.User, text));
        }
    }

    public void AddAssistantMessage(string text)
    {
        if (!string.IsNullOrWhiteSpace(text))
        {
            _messages.Add(new ChatMessage(ChatRole.Assistant, text));
        }
    }
}
