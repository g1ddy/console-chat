using Microsoft.Extensions.AI;

using SemanticKernelChat;
using SemanticKernelChat.Console;
using SemanticKernelChat.Infrastructure;

using ConsoleChat.Tests.TestUtilities;

using Spectre.Console;
using Spectre.Console.Testing;

namespace ConsoleChat.Tests;

public class ChatConsoleTests
{

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

        var msg = new ChatMessage(ChatRole.User, "hello");
        var console = new ChatConsole(new ChatLineEditor(new McpToolCollection()), testConsole);
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

        var console = new ChatConsole(new ChatLineEditor(new McpToolCollection()), testConsole);
        console.WriteChatMessages(callMessage, resultMessage);

        Assert.Contains("First", testConsole.Output);
    }

    [Fact]
    public async Task SendAndDisplayAsync_Writes_Response()
    {
        var history = new ChatHistoryService();
        history.AddUserMessage("hi");

        var testConsole = new TestConsole();

        var client = new FakeChatClient { Response = new(new ChatMessage(ChatRole.Assistant, "done")) };
        var console = new ChatConsole(new ChatLineEditor(new McpToolCollection()), testConsole);
        var controller = new ChatController(console, client, new McpToolCollection());
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

        var console = new ChatConsole(new ChatLineEditor(new McpToolCollection()), testConsole);
        var controller = new ChatController(console, client, new McpToolCollection());
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

        var console = new ChatConsole(new ChatLineEditor(new McpToolCollection()), testConsole);
        _ = await console.DisplayStreamingUpdatesAsync(updates);

        Assert.Contains("First", testConsole.Output);
        Assert.Contains("Second", testConsole.Output);
    }

    [Fact]
    public void DisplayError_Writes_Exception_Message()
    {
        var testConsole = new TestConsole();
        var console = new ChatConsole(new ChatLineEditor(new McpToolCollection()), testConsole);

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

        var console = new ChatConsole(new ChatLineEditor(new McpToolCollection()), testConsole);
        _ = await console.DisplayStreamingUpdatesAsync(updates);

        Assert.Contains("Tool Result", testConsole.Output);
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
