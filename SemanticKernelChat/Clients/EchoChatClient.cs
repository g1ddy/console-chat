using System.Runtime.CompilerServices;
using System.Collections.Generic;
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
        await Task.Delay(500, cancellationToken);

        var responseContents = new List<AIContent>();
        var lastMessage = messages.Last();

        if (lastMessage.Role == ChatRole.User)
        {
            responseContents.Add(new TextContent("I need to call some tools!"));
            responseContents.Add(new FunctionCallContent("tool_call_table", "RenderableFunctions_SampleTable"));
            responseContents.Add(new FunctionCallContent("tool_call_tree", "RenderableFunctions_SampleTree"));
            responseContents.Add(new FunctionCallContent("tool_call_chart", "RenderableFunctions_SampleChart"));
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
        var lastMessage = messages.Last();

        if (lastMessage.Role == ChatRole.User)
        {
            var text = "I need to call some tools!";
            foreach (char c in text)
            {
                await Task.Delay(100, cancellationToken);
                yield return new ChatResponseUpdate(ChatRole.Assistant, c.ToString());
            }

            var callContents = new List<AIContent>
            {
                new FunctionCallContent("tool_call_table", "RenderableFunctions_SampleTable"),
                new FunctionCallContent("tool_call_tree", "RenderableFunctions_SampleTree"),
                new FunctionCallContent("tool_call_chart", "RenderableFunctions_SampleChart")
            };

            await Task.Delay(100, cancellationToken);
            yield return new ChatResponseUpdate(ChatRole.Assistant, callContents);
        }
        else if (lastMessage.Role == ChatRole.Tool)
        {
            var text = "I got what I need!" + Environment.NewLine;
            foreach (char c in text)
            {
                await Task.Delay(100, cancellationToken);
                yield return new ChatResponseUpdate(ChatRole.Assistant, c.ToString());
            }

            foreach (var result in lastMessage.Contents.OfType<FunctionResultContent>())
            {
                var lines = new[]
                {
                    $"Tool id: {result.CallId}" + Environment.NewLine,
                    $"Tool result: {result.Result}" + Environment.NewLine
                };

                foreach (var line in lines)
                {
                    await Task.Delay(200, cancellationToken);
                    yield return new ChatResponseUpdate(ChatRole.Assistant, line);
                }
            }
        }
        else
        {
            var text = lastMessage.Text;
            foreach (char c in text)
            {
                await Task.Delay(100, cancellationToken);
                yield return new ChatResponseUpdate(ChatRole.Assistant, c.ToString());
            }
        }
    }

    /// <inheritdoc />
    public object? GetService(Type serviceType, object? serviceKey) => null;

    void IDisposable.Dispose()
    {
        // Nothing to dispose
    }
}
