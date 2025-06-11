using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.AI;

namespace SemanticKernelChat;

/// <summary>
/// Test implementation of <see cref="IChatClient"/> that echoes the user's last message.
/// </summary>
public sealed class EchoChatClient : IChatClient
{
    /// <inheritdoc />
    public async Task<ChatResponse> GetResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default)
    {
        var text = messages.Last().Text;
        // Simulate a response delay like a real AI model would have
        await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
        var responseMessage = new ChatMessage(ChatRole.Assistant, text);
        return new ChatResponse(responseMessage);
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var text = messages.Last().Text;
        foreach (char c in text)
        {
            await Task.Delay(100, cancellationToken);
            yield return new ChatResponseUpdate(ChatRole.Assistant, c.ToString());
        }
    }

    /// <inheritdoc />
    public object? GetService(Type serviceType, object? serviceKey) => null;

    void IDisposable.Dispose()
    {
        // Nothing to dispose
    }
}
