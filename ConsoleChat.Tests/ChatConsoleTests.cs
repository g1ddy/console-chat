using ConsoleChat.Tests.TestUtilities;

using Microsoft.Extensions.AI;

using SemanticKernelChat;
using SemanticKernelChat.Console;

using Spectre.Console;
using Spectre.Console.Testing;

namespace ConsoleChat.Tests;

public class ChatConsoleTests
{

    [Fact]
    public void GetHeaderStyle_Returns_Values_For_User()
    {
        var (header, j, style) = ChatConsoleHelpers.GetHeaderStyle(ChatRole.User);
        Assert.StartsWith(":bust_in_silhouette: User", header);
        Assert.Equal(Justify.Left, j);
        Assert.Equal(Color.RoyalBlue1, style.Foreground);
    }

    [Fact]
    public void GetHeaderStyle_Returns_Values_For_Assistant()
    {
        var (header, j, style) = ChatConsoleHelpers.GetHeaderStyle(ChatRole.Assistant);
        Assert.StartsWith(":robot: Assistant", header);
        Assert.Equal(Justify.Right, j);
        Assert.Equal(Color.DarkSeaGreen2, style.Foreground);
    }

    [Fact]
    public void GetHeaderStyle_Returns_Values_For_Tool()
    {
        var (header, j, style) = ChatConsoleHelpers.GetHeaderStyle(ChatRole.Tool);
        Assert.StartsWith(":wrench: Tool", header);
        Assert.Equal(Justify.Center, j);
        Assert.Equal(Color.Grey37, style.Foreground);
    }

    [Fact]
    public void WriteChatMessages_Writes_To_Console()
    {
        var history = new ChatHistoryService();
        var testConsole = new TestConsole();

        var msg = new ChatMessage(ChatRole.User, "hello");
        var completion = new CommandCompletion(Enumerable.Empty<IChatCommandStrategy>());
        var console = new ChatConsole(new ChatLineEditor(completion), testConsole);
        console.WriteChatMessages(msg);

        Assert.Empty(history.Messages);
        Assert.Contains("hello", testConsole.Output);
    }

    [Fact]
    public void WriteChatMessages_Shows_Tool_Names_For_Results()
    {
        var testConsole = new TestConsole();

        var callMessage = new ChatMessage(ChatRole.Assistant, new AIContent[]
        {
            new FunctionCallContent("1", "First", new Dictionary<string, object?>())
        });

        var resultMessage = new ChatMessage(ChatRole.Tool, new AIContent[]
        {
            new FunctionResultContent("1", "r1")
        });

        var completion = new CommandCompletion(Enumerable.Empty<IChatCommandStrategy>());
        var console = new ChatConsole(new ChatLineEditor(completion), testConsole);
        console.WriteChatMessages(callMessage, resultMessage);

        Assert.Contains("First", testConsole.Output);
    }

    [Fact]
    public void WriteChatMessages_DebugJson_Embedded_In_Panel()
    {
        var testConsole = new TestConsole();

        var resultMessage = new ChatMessage(ChatRole.Tool, new AIContent[]
        {
            new FunctionResultContent("id", "{\"a\":1}")
        });

        var completion = new CommandCompletion(Enumerable.Empty<IChatCommandStrategy>());
        var console = new ChatConsole(new ChatLineEditor(completion), testConsole)
        {
            DebugEnabled = true
        };

        console.WriteChatMessages(resultMessage);

        Assert.Contains("\"a\": 1", testConsole.Output);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void WriteChatMessages_Call_Parameters_Visibility(bool debugEnabled)
    {
        var testConsole = new TestConsole();

        var callMessage = new ChatMessage(ChatRole.Assistant, new AIContent[]
        {
            new FunctionCallContent("id", "Tool", new Dictionary<string, object?> { ["p"] = 1 })
        });

        var completion = new CommandCompletion(Enumerable.Empty<IChatCommandStrategy>());
        var console = new ChatConsole(new ChatLineEditor(completion), testConsole)
        {
            DebugEnabled = debugEnabled
        };

        console.WriteChatMessages(callMessage);

        if (debugEnabled)
        {
            Assert.Contains("\"p\": 1", testConsole.Output);
        }
        else
        {
            Assert.DoesNotContain("\"p\": 1", testConsole.Output);
        }
    }

    [Fact]
    public void WriteChatMessages_Debug_Call_Parameters()
    {
        var testConsole = new TestConsole();

        var callMessage = new ChatMessage(ChatRole.Assistant, new AIContent[]
        {
            new FunctionCallContent("id", "Tool", new Dictionary<string, object?> { ["p"] = 1 })
        });

        var completion = new CommandCompletion(Enumerable.Empty<IChatCommandStrategy>());
        var console = new ChatConsole(new ChatLineEditor(completion), testConsole)
        {
            DebugEnabled = true
        };

        console.WriteChatMessages(callMessage);

        Assert.Contains("\"p\": 1", testConsole.Output);
    }

    [Fact]
    public void WriteChatMessages_No_Debug_Hides_Call_Parameters()
    {
        var testConsole = new TestConsole();

        var callMessage = new ChatMessage(ChatRole.Assistant, new AIContent[]
        {
            new FunctionCallContent("id", "Tool", new Dictionary<string, object?> { ["p"] = 1 })
        });

        var completion = new CommandCompletion(Enumerable.Empty<IChatCommandStrategy>());
        var console = new ChatConsole(new ChatLineEditor(completion), testConsole);

        console.WriteChatMessages(callMessage);

        Assert.DoesNotContain("\"p\": 1", testConsole.Output);
    }

    [Fact]
    public async Task SendAndDisplayAsync_Writes_Response()
    {
        var history = new ChatHistoryService();
        history.AddUserMessage("hi");

        var testConsole = new TestConsole();

        var client = new FakeChatClient { Response = new(new ChatMessage(ChatRole.Assistant, "done")) };
        var completion = new CommandCompletion(Enumerable.Empty<IChatCommandStrategy>());
        var console = new ChatConsole(new ChatLineEditor(completion), testConsole);
        var controller = new ChatController(console, client, McpCollectionFactory.CreateToolCollection(), []);
        await controller.SendAndDisplayAsync(history);

        Assert.Equal(2, history.Messages.Count);
        Assert.Contains("done", testConsole.Output);
    }

    [Fact]
    public async Task SendAndDisplayStreamingAsync_Writes_Updates()
    {
        var history = new ChatHistoryService();
        history.AddUserMessage("hi");

        var testConsole = new TestConsole();

        var client = new FakeChatClient();
        client.StreamingUpdates.Add(new ChatResponseUpdate(ChatRole.Assistant, "A"));
        client.StreamingUpdates.Add(new ChatResponseUpdate(ChatRole.Assistant, "B"));

        var completion = new CommandCompletion(Enumerable.Empty<IChatCommandStrategy>());
        var console = new ChatConsole(new ChatLineEditor(completion), testConsole);
        var controller = new ChatController(console, client, McpCollectionFactory.CreateToolCollection(), []);
        await controller.SendAndDisplayStreamingAsync(history);

        Assert.Equal(2, history.Messages.Count);
        Assert.Contains("AB", history.Messages.Last().Text);
        Assert.Contains("AB", testConsole.Output);
    }

    [Fact]
    public async Task DisplayStreamingUpdatesAsync_Logs_Multiple_Tool_Results()
    {
        var testConsole = new TestConsole();

        var callContents = new List<AIContent>
        {
            new FunctionCallContent("1", "First", new Dictionary<string, object?>()),
            new FunctionCallContent("2", "Second", new Dictionary<string, object?>())
        };

        var resultContents = new List<AIContent>
        {
            new FunctionResultContent("1", "r1"),
            new FunctionResultContent("2", "r2")
        };

        var updates = AsAsyncEnumerable([
            new ChatResponseUpdate(ChatRole.Assistant, callContents),
            new ChatResponseUpdate(ChatRole.Tool, resultContents)
        ]);

        var completion = new CommandCompletion(Enumerable.Empty<IChatCommandStrategy>());
        var console = new ChatConsole(new ChatLineEditor(completion), testConsole);
        _ = await console.DisplayStreamingUpdatesAsync(updates);

        Assert.Contains("First", testConsole.Output);
        Assert.Contains("Second", testConsole.Output);
    }

    [Fact]
    public void DisplayError_Writes_Exception_Message()
    {
        var testConsole = new TestConsole();
        var completion = new CommandCompletion(Enumerable.Empty<IChatCommandStrategy>());
        var console = new ChatConsole(new ChatLineEditor(completion), testConsole);

        console.DisplayError(new InvalidOperationException("fail"));

        Assert.Contains("fail", testConsole.Output);
    }

    [Fact]
    public async Task DisplayStreamingUpdatesAsync_Shows_Default_Tool_Name()
    {
        var testConsole = new TestConsole();

        var updates = AsAsyncEnumerable([
            new ChatResponseUpdate(ChatRole.Tool, new[] { new FunctionResultContent("id", "r") })
        ]);

        var completion = new CommandCompletion(Enumerable.Empty<IChatCommandStrategy>());
        var console = new ChatConsole(new ChatLineEditor(completion), testConsole);
        _ = await console.DisplayStreamingUpdatesAsync(updates);

        Assert.Contains("Tool Result", testConsole.Output);
    }

    [Fact]
    public async Task DisplayStreamingUpdatesAsync_DebugJson_Embedded()
    {
        var testConsole = new TestConsole();

        var updates = AsAsyncEnumerable([
            new ChatResponseUpdate(ChatRole.Tool, new[] { new FunctionResultContent("id", "{\"x\":2}") })
        ]);

        var completion = new CommandCompletion(Enumerable.Empty<IChatCommandStrategy>());
        var console = new ChatConsole(new ChatLineEditor(completion), testConsole)
        {
            DebugEnabled = true
        };

        _ = await console.DisplayStreamingUpdatesAsync(updates);

        Assert.Contains("\"x\": 2", testConsole.Output);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task DisplayStreamingUpdatesAsync_Call_Parameters_Visibility(bool debugEnabled)
    {
        var testConsole = new TestConsole();

        var updates = AsAsyncEnumerable([
            new ChatResponseUpdate(ChatRole.Assistant, new[] { new FunctionCallContent("id", "Tool", new Dictionary<string, object?> { ["v"] = 5 }) })
        ]);

        var completion = new CommandCompletion(Enumerable.Empty<IChatCommandStrategy>());
        var console = new ChatConsole(new ChatLineEditor(completion), testConsole)
        {
            DebugEnabled = debugEnabled
        };

        _ = await console.DisplayStreamingUpdatesAsync(updates);

        if (debugEnabled)
        {
            Assert.Contains("\"v\": 5", testConsole.Output);
        }
        else
        {
            Assert.DoesNotContain("\"v\": 5", testConsole.Output);
        }
    }


    private static async IAsyncEnumerable<ChatResponseUpdate> AsAsyncEnumerable(IEnumerable<ChatResponseUpdate> updates)
    {
        foreach (var update in updates)
        {
            yield return update;
            await Task.Yield();
        }
    }
}
