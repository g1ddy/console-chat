using System.Runtime.CompilerServices;
using Microsoft.Extensions.AI;

namespace SemanticKernelChat.Clients;

/// <summary>
/// Test implementation of <see cref="IChatClient"/> that echoes the user's last message.
/// </summary>
public sealed class EchoChatClient : IChatClient
{
    /// <inheritdoc />
    public async Task<ChatResponse> GetResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default)
    {
        // Simulate a response delay like a real AI model would have
        await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);

        var responseContents = new List<AIContent>();
        var lastMessage = messages.Last();

        if (lastMessage.Role == ChatRole.User)
        {
            responseContents.Add(new TextContent("I need to call some tools!"));
            responseContents.Add(new FunctionCallContent("tool_call_time", "CurrentTime"));
            responseContents.Add(new FunctionCallContent("tool_call_echo", "ReverseEcho", new Dictionary<string, object?>
            {
                { "message", lastMessage.Text }
            }));
        }
        else if (lastMessage.Role == ChatRole.Tool)
        {
            // If the last message was a tool response, we assume it has a function result
            responseContents.Add(new TextContent("I got what I need!" + Environment.NewLine));
            foreach (var result in lastMessage.Contents.OfType<FunctionResultContent>())
            {
                responseContents.Add(new TextContent($"Tool id: {result.CallId}" + Environment.NewLine));
                responseContents.Add(new TextContent($"Tool result: {result.Result}" + Environment.NewLine));

            }
        }

        var responseMessage = new ChatMessage(ChatRole.Assistant, responseContents);
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
