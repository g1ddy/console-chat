using System.Runtime.CompilerServices;

using Microsoft.Extensions.AI;

using SemanticKernelChat.Plugins;

using Spectre.Console;

using static SemanticKernelChat.Plugins.RenderableFunctions;

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
                    new ItemCount("Apples", 12),
                    new ItemCount("Bananas", 7)
                }
            };

            var treeData = new RenderableFunctions.TreeNode("Root");
            var leaf = new RenderableFunctions.TreeNode("Leaf");
            var branch1 = new RenderableFunctions.TreeNode("Branch 1");
            branch1.AddChild(leaf);
            treeData.AddChild(branch1);

            var branch2 = new RenderableFunctions.TreeNode("Branch 2");
            treeData.AddChild(branch2);

            var treeParams = new Dictionary<string, object?>
            {
                ["root"] = treeData
            };

            var chartParams = new Dictionary<string, object?>
            {
                ["items"] = new[]
                {
                    new ChartItem("Apples", 12, Color.Red),
                    new ChartItem("Bananas", 7, Color.Yellow)
                },
                ["title"] = "Fruit Sales"
            };

            var callContents = new List<AIContent>
            {
                new FunctionCallContent("tool_call_table", "RenderableFunctions_RenderTable", tableParams),
                new FunctionCallContent("tool_call_tree", "RenderableFunctions_RenderTree", treeParams),
                new FunctionCallContent("tool_call_chart", "RenderableFunctions_RenderChart", chartParams),
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
