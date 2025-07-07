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

            var tableParams = new Dictionary<string, object?>
            {
                ["items"] = new[]
                {
                    new Dictionary<string, object?> { ["Name"] = "Apples", ["Count"] = 12 },
                    new Dictionary<string, object?> { ["Name"] = "Bananas", ["Count"] = 7 }
                }
            };

            var treeParams = new Dictionary<string, object?>
            {
                ["root"] = new Dictionary<string, object?>
                {
                    ["Name"] = "Root",
                    ["Children"] = new[]
                    {
                        new Dictionary<string, object?>
                        {
                            ["Name"] = "Branch 1",
                            ["Children"] = new[]
                            {
                                new Dictionary<string, object?> { ["Name"] = "Leaf" }
                            }
                        },
                        new Dictionary<string, object?> { ["Name"] = "Branch 2" }
                    }
                }
            };

            var chartParams = new Dictionary<string, object?>
            {
                ["items"] = new[]
                {
                    new Dictionary<string, object?> { ["Name"] = "Apples", ["Value"] = 12, ["Color"] = "Red" },
                    new Dictionary<string, object?> { ["Name"] = "Bananas", ["Value"] = 7, ["Color"] = "Yellow" }
                },
                ["title"] = "Fruit Sales"
            };

            var callContents = new List<AIContent>
            {
                new FunctionCallContent("tool_call_table", "RenderableFunctions_SampleTable", tableParams),
                new FunctionCallContent("tool_call_tree", "RenderableFunctions_SampleTree", treeParams),
                new FunctionCallContent("tool_call_chart", "RenderableFunctions_SampleChart", chartParams)
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
