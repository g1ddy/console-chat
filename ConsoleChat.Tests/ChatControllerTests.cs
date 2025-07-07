using ConsoleChat.Tests.TestUtilities;

using Microsoft.Extensions.AI;

using NSubstitute;

using SemanticKernelChat;
using SemanticKernelChat.Console;

namespace ConsoleChat.Tests;

public class ChatControllerTests
{

    [Fact]
    public async Task SendAndDisplayAsync_Adds_To_History_And_Writes_To_Console()
    {
        var history = new ChatHistoryService();
        history.AddUserMessage("hi");

        var console = Substitute.For<IChatConsole>();
        console.DisplayThinkingIndicator(Arg.Any<Func<Task>>())
            .Returns(call => ((Func<Task>)call[0])());

        var client = new FakeChatClient { Response = new(new ChatMessage(ChatRole.Assistant, "done")) };
        var controller = new ChatController(console, client, McpCollectionFactory.CreateToolCollection(), []);

        await controller.SendAndDisplayAsync(history);

        Assert.Equal(2, history.Messages.Count);
        await console.Received(1).DisplayThinkingIndicator(Arg.Any<Func<Task>>());
        console.Received(1).WriteChatMessages(Arg.Is<ChatMessage[]>(msgs => msgs.Length == 1 && msgs[0].Text == "done"));
        console.DidNotReceive().DisplayError(Arg.Any<Exception>());
    }

    [Fact]
    public async Task SendAndDisplayAsync_On_Error_Displays_Error()
    {
        var history = new ChatHistoryService();
        history.AddUserMessage("hi");

        var console = Substitute.For<IChatConsole>();
        console.DisplayThinkingIndicator(Arg.Any<Func<Task>>())
            .Returns(call => ((Func<Task>)call[0])());

        var client = Substitute.For<IChatClient>();
        client
            .GetResponseAsync(Arg.Any<IEnumerable<ChatMessage>>(), Arg.Any<ChatOptions?>(), Arg.Any<CancellationToken>())
            .Returns(call => Task.FromException<ChatResponse>(new InvalidOperationException("fail")));

        var controller = new ChatController(console, client, McpCollectionFactory.CreateToolCollection(), []);

        await controller.SendAndDisplayAsync(history);

        await console.Received(1).DisplayThinkingIndicator(Arg.Any<Func<Task>>());
        console.Received(1).DisplayError(Arg.Any<Exception>());
        console.DidNotReceive().WriteChatMessages(Arg.Any<ChatMessage[]>());
        Assert.Single(history.Messages);
    }

    [Fact]
    public async Task SendAndDisplayStreamingAsync_Adds_History_And_Fires_Callback()
    {
        var history = new ChatHistoryService();
        history.AddUserMessage("hi");

        var client = new FakeChatClient();
        client.StreamingUpdates.Add(new ChatResponseUpdate(ChatRole.Assistant, "A"));
        client.StreamingUpdates.Add(new ChatResponseUpdate(ChatRole.Assistant, "B"));

        var console = Substitute.For<IChatConsole>();
        console.DisplayStreamingUpdatesAsync(Arg.Any<IAsyncEnumerable<ChatResponseUpdate>>())
            .Returns(Task.FromResult<IReadOnlyList<ChatMessage>>(new[] { new ChatMessage(ChatRole.Assistant, "AB") }));

        var controller = new ChatController(console, client, McpCollectionFactory.CreateToolCollection(), []);
        IReadOnlyList<ChatMessage>? finalMessages = null;

        await controller.SendAndDisplayStreamingAsync(history, msgs => finalMessages = msgs);

        _ = await console.Received(1).DisplayStreamingUpdatesAsync(Arg.Any<IAsyncEnumerable<ChatResponseUpdate>>());
        console.DidNotReceive().DisplayError(Arg.Any<Exception>());
        Assert.Equal(2, history.Messages.Count);
        Assert.Equal("AB", history.Messages.Last().Text);
        Assert.Equal("AB", finalMessages?.First().Text);
    }

    [Fact]
    public async Task SendAndDisplayStreamingAsync_On_Error_Displays_Error()
    {
        var history = new ChatHistoryService();
        history.AddUserMessage("hi");

        var client = new FakeChatClient();
        client.StreamingUpdates.Add(new ChatResponseUpdate(ChatRole.Assistant, "A"));

        var console = Substitute.For<IChatConsole>();
        console
            .DisplayStreamingUpdatesAsync(Arg.Any<IAsyncEnumerable<ChatResponseUpdate>>())
            .Returns(call => Task.FromException<IReadOnlyList<ChatMessage>>(new InvalidOperationException("fail")));

        var controller = new ChatController(console, client, McpCollectionFactory.CreateToolCollection(), []);

        await controller.SendAndDisplayStreamingAsync(history);

        _ = await console.Received(1).DisplayStreamingUpdatesAsync(Arg.Any<IAsyncEnumerable<ChatResponseUpdate>>());
        console.Received(1).DisplayError(Arg.Any<Exception>());
        Assert.Single(history.Messages);
    }

    [Fact]
    public async Task SendAndDisplayAsync_Summarizes_When_Threshold_Exceeded()
    {
        var history = new ChatHistoryService();
        for (int i = 0; i < 3; i++)
        {
            history.AddUserMessage($"u{i}");
            history.AddAssistantMessage($"a{i}");
        }
        history.AddUserMessage("hello");

        var console = Substitute.For<IChatConsole>();
        console.DisplayThinkingIndicator(Arg.Any<Func<Task>>())
            .Returns(call => ((Func<Task>)call[0])());

        var calls = new List<IEnumerable<ChatMessage>>();
        var client = Substitute.For<IChatClient>();
        client
            .GetResponseAsync(Arg.Any<IEnumerable<ChatMessage>>(), Arg.Any<ChatOptions?>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                calls.Add((IEnumerable<ChatMessage>)call[0]);
                if (calls.Count == 1)
                {
                    return Task.FromResult(new ChatResponse(new ChatMessage(ChatRole.Assistant, "summary")));
                }
                return Task.FromResult(new ChatResponse(new ChatMessage(ChatRole.Assistant, "done")));
            });

        var controller = new ChatController(console, client, McpCollectionFactory.CreateToolCollection(), [], summaryThreshold:5, summaryKeepLast:2);

        await controller.SummarizeAsync(history);
        await controller.SendAndDisplayAsync(history);

        Assert.Equal(4, history.Messages.Count);
        Assert.Equal("summary", history.Messages[0].Text);
        await client.Received(2).GetResponseAsync(Arg.Any<IEnumerable<ChatMessage>>(), Arg.Any<ChatOptions?>(), Arg.Any<CancellationToken>());
        console.Received(1).WriteChatMessages(Arg.Is<ChatMessage[]>(msgs => msgs.Last().Text == "done"));
    }
}
