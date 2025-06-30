namespace ConsoleChat.Tests.TestUtilities;

using System.Runtime.CompilerServices;
using Microsoft.Extensions.AI;
using SemanticKernelChat;

public sealed class FakeChatClient : IChatClient
{
    public ChatResponse Response { get; set; } = new(new ChatMessage(ChatRole.Assistant, "reply"));

    public List<ChatResponseUpdate> StreamingUpdates { get; } = new();

    public IReadOnlyList<ChatMessage>? LastMessages { get; private set; }
    public IReadOnlyList<ChatMessage>? LastStreamingMessages { get; private set; }

    public Task<ChatResponse> GetResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default)
    {
        LastMessages = messages.ToList();
        return Task.FromResult(Response);
    }

    public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        LastStreamingMessages = messages.ToList();
        foreach (var update in StreamingUpdates)
        {
            yield return update;
            await Task.Yield();
        }
    }

    public object? GetService(Type serviceType, object? serviceKey) => null;

    public void Dispose() { }
}
