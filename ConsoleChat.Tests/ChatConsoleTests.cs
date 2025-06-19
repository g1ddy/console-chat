using System.Runtime.CompilerServices;
using AI = Microsoft.Extensions.AI;
using ModelContextProtocol.Client;
using Spectre.Console;
using Spectre.Console.Testing;
using SemanticKernelChat;
using System.Collections.Generic;
using Microsoft.SemanticKernel;

namespace ConsoleChat.Tests;

public class ChatConsoleTests
{
    private sealed class FakeChatClient : AI.IChatClient
    {
        public AI.ChatResponse Response { get; set; } = new(new AI.ChatMessage(AI.ChatRole.Assistant, "reply"));
        public List<AI.ChatResponseUpdate> StreamingUpdates { get; } = new();

        public Task<AI.ChatResponse> GetResponseAsync(IEnumerable<AI.ChatMessage> messages, AI.ChatOptions? options = null, CancellationToken cancellationToken = default)
            => Task.FromResult(Response);

        public async IAsyncEnumerable<AI.ChatResponseUpdate> GetStreamingResponseAsync(IEnumerable<AI.ChatMessage> messages, AI.ChatOptions? options = null, [EnumeratorCancellation] CancellationToken cancellationToken = default)
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

    [Fact]
    public void GetUserStyle_Returns_Values_For_User()
    {
        var (header, j, style) = SemanticKernelChat.Console.ChatConsole.GetUserStyle(AI.ChatRole.User);
        Assert.StartsWith(":bust_in_silhouette: User", header);
        Assert.Equal(Justify.Left, j);
        Assert.Equal(Color.RoyalBlue1, style.Foreground);
    }

    [Fact]
    public void GetUserStyle_Returns_Values_For_Assistant()
    {
        var (header, j, style) = SemanticKernelChat.Console.ChatConsole.GetUserStyle(AI.ChatRole.Assistant);
        Assert.StartsWith(":robot: Assistant", header);
        Assert.Equal(Justify.Right, j);
        Assert.Equal(Color.DarkSeaGreen2, style.Foreground);
    }

    [Fact]
    public void GetUserStyle_Returns_Values_For_Tool()
    {
        var (header, j, style) = SemanticKernelChat.Console.ChatConsole.GetUserStyle(AI.ChatRole.Tool);
        Assert.StartsWith(":wrench: Tool", header);
        Assert.Equal(Justify.Center, j);
        Assert.Equal(Color.Grey37, style.Foreground);
    }

    [Fact]
    public void WriteChatMessages_Writes_To_Console()
    {
        var history = new ChatHistoryService();
        var testConsole = new TestConsole();
        AnsiConsole.Console = testConsole;

        var msg = new AI.ChatMessage(AI.ChatRole.User, "hello");
        var kernel = Kernel.CreateBuilder().Build();
        var console = new SemanticKernelChat.Console.ChatConsole(new SemanticKernelChat.Console.ChatLineEditor(kernel));
        console.WriteChatMessages(msg);

        Assert.Empty(history.Messages);
        Assert.Contains("hello", testConsole.Output);
    }

    [Fact]
    public async Task SendAndDisplayAsync_Writes_Response()
    {
        var history = new ChatHistoryService();
        history.AddUserMessage("hi");

        var testConsole = new TestConsole();
        AnsiConsole.Console = testConsole;

        var client = new FakeChatClient { Response = new(new AI.ChatMessage(AI.ChatRole.Assistant, "done")) };
        var kernel2 = Kernel.CreateBuilder().Build();
        var console = new SemanticKernelChat.Console.ChatConsole(new SemanticKernelChat.Console.ChatLineEditor(kernel2));
        var controller = new SemanticKernelChat.Console.ChatController(console);
        await controller.SendAndDisplayAsync(client, history, Array.Empty<McpClientTool>());

        Assert.Equal(2, history.Messages.Count);
        Assert.Contains("done", testConsole.Output);
    }

    [Fact]
    public async Task SendAndDisplayStreamingAsync_Writes_Updates()
    {
        var history = new ChatHistoryService();
        history.AddUserMessage("hi");

        var testConsole = new TestConsole();
        AnsiConsole.Console = testConsole;

        var client = new FakeChatClient();
        client.StreamingUpdates.Add(new AI.ChatResponseUpdate(AI.ChatRole.Assistant, "A"));
        client.StreamingUpdates.Add(new AI.ChatResponseUpdate(AI.ChatRole.Assistant, "B"));

        var kernel3 = Kernel.CreateBuilder().Build();
        var console = new SemanticKernelChat.Console.ChatConsole(new SemanticKernelChat.Console.ChatLineEditor(kernel3));
        var controller = new SemanticKernelChat.Console.ChatController(console);
        await controller.SendAndDisplayStreamingAsync(client, history, Array.Empty<McpClientTool>());

        Assert.Equal(2, history.Messages.Count);
        Assert.Contains("AB", history.Messages.Last().Text);
        Assert.Contains("AB", testConsole.Output);
    }

    [Fact]
    public async Task DisplayStreamingUpdatesAsync_Logs_Multiple_Tool_Results()
    {
        var testConsole = new TestConsole();
        AnsiConsole.Console = testConsole;

        var callContents = new List<AI.AIContent>
        {
            new AI.FunctionCallContent("1", "First", new Dictionary<string, object?>()),
            new AI.FunctionCallContent("2", "Second", new Dictionary<string, object?>())
        };

        var resultContents = new List<AI.AIContent>
        {
            new AI.FunctionResultContent("1", "r1"),
            new AI.FunctionResultContent("2", "r2")
        };

        var updates = AsAsyncEnumerable([
            new AI.ChatResponseUpdate(AI.ChatRole.Assistant, callContents),
            new AI.ChatResponseUpdate(AI.ChatRole.Tool, resultContents)
        ]);

        var kernel4 = Kernel.CreateBuilder().Build();
        var console = new SemanticKernelChat.Console.ChatConsole(new SemanticKernelChat.Console.ChatLineEditor(kernel4));
        _ = await console.DisplayStreamingUpdatesAsync(updates);

        Assert.Contains("First", testConsole.Output);
        Assert.Contains("Second", testConsole.Output);
    }

    private static async IAsyncEnumerable<AI.ChatResponseUpdate> AsAsyncEnumerable(IEnumerable<AI.ChatResponseUpdate> updates)
    {
        foreach (var update in updates)
        {
            yield return update;
            await Task.Yield();
        }
    }
}
