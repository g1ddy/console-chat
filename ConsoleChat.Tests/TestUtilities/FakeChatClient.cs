namespace ConsoleChat.Tests.TestUtilities;

using System.Runtime.CompilerServices;
using Microsoft.Extensions.AI;
using SemanticKernelChat;

public sealed class FakeChatClient : IChatClient
{
    public ChatResponse Response { get; set; } = new(new ChatMessage(ChatRole.Assistant, "reply"));

    public List<ChatResponseUpdate> StreamingUpdates { get; } = new();

    public Task<ChatResponse> GetResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default)
        => Task.FromResult(Response);

    public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        foreach (var update in StreamingUpdates)
        {
            yield return update;
            await Task.Yield();
        }
    }

    public object? GetService(Type serviceType, object? serviceKey) => null;

    public void Dispose() { }
}
