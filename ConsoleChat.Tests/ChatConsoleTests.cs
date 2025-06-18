using System.Runtime.CompilerServices;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;
using Spectre.Console;
using Spectre.Console.Testing;
using SemanticKernelChat;

namespace ConsoleChat.Tests;

public class ChatConsoleTests
{
    private sealed class FakeChatClient : IChatClient
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

    [Fact]
    public void GetUserStyle_Returns_Values_For_User()
    {
        var (header, j, style) = SemanticKernelChat.Console.ChatConsole.GetUserStyle(ChatRole.User);
        Assert.StartsWith(":bust_in_silhouette: User", header);
        Assert.Equal(Justify.Left, j);
        Assert.Equal(Color.RoyalBlue1, style.Foreground);
    }

    [Fact]
    public void GetUserStyle_Returns_Values_For_Assistant()
    {
        var (header, j, style) = SemanticKernelChat.Console.ChatConsole.GetUserStyle(ChatRole.Assistant);
        Assert.StartsWith(":robot: Assistant", header);
        Assert.Equal(Justify.Right, j);
        Assert.Equal(Color.DarkSeaGreen2, style.Foreground);
    }

    [Fact]
    public void GetUserStyle_Returns_Values_For_Tool()
    {
        var (header, j, style) = SemanticKernelChat.Console.ChatConsole.GetUserStyle(ChatRole.Tool);
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

        var msg = new ChatMessage(ChatRole.User, "hello");
        SemanticKernelChat.Console.ChatConsole.WriteChatMessages(history, msg);

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

        var client = new FakeChatClient { Response = new(new ChatMessage(ChatRole.Assistant, "done")) };
        await SemanticKernelChat.Console.ChatController.SendAndDisplayAsync(client, history, Array.Empty<McpClientTool>());

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
        client.StreamingUpdates.Add(new ChatResponseUpdate(ChatRole.Assistant, "A"));
        client.StreamingUpdates.Add(new ChatResponseUpdate(ChatRole.Assistant, "B"));

        await SemanticKernelChat.Console.ChatController.SendAndDisplayStreamingAsync(client, history, Array.Empty<McpClientTool>());

        Assert.Equal(2, history.Messages.Count);
        Assert.Contains("AB", history.Messages.Last().Text);
        Assert.Contains("AB", testConsole.Output);
    }
}
