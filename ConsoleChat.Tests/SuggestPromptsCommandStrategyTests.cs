using System.Collections.Concurrent;
using ConsoleChat.Tests.TestUtilities;
using Microsoft.Extensions.AI;
using ModelContextProtocol;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using NSubstitute;
using SemanticKernelChat;
using SemanticKernelChat.Console;
using SemanticKernelChat.Infrastructure;

namespace ConsoleChat.Tests;

public class SuggestPromptsCommandStrategyTests
{
    private static McpPromptCollection CreateCollection() =>
        PromptFactory.CreateCollectionWithPrompts("One", "Two", "Three");

    [Fact]
    public void GetCompletions_Returns_Command_Name()
    {
        var strategy = new SuggestPromptsCommandStrategy(new FakeChatClient(), CreateCollection());
        var completions = strategy.GetCompletions("", "/s", "");

        Assert.NotNull(completions);
        Assert.Contains(CliConstants.Commands.Suggest, completions!);
    }

    [Fact]
    public void CanExecute_Returns_True_For_Command()
    {
        var strategy = new SuggestPromptsCommandStrategy(new FakeChatClient(), CreateCollection());
        Assert.True(strategy.CanExecute("/suggest"));
    }

    [Fact]
    public async Task ExecuteAsync_Calls_ChatClient()
    {
        var client = new FakeChatClient { Response = new(new ChatMessage(ChatRole.Assistant, "One\nTwo\nThree")) };
        var strategy = new SuggestPromptsCommandStrategy(client, CreateCollection());
        var history = new ChatHistoryService();
        var console = Substitute.For<IChatConsole>();

        await strategy.ExecuteAsync("/suggest", history, Substitute.For<IChatController>(), console);

        Assert.NotNull(client.LastMessages);
        Assert.Contains("Suggest", client.LastMessages!.Last().Text, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_WhenGetAsyncThrows_CallsDisplayError()
    {
        // Arrange
        var expectedException = new Exception("Prompt retrieval failed");
        var mcpClient = Substitute.For<McpClient>();
        mcpClient.SendRequestAsync(Arg.Any<JsonRpcRequest>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<JsonRpcResponse>(expectedException));

        var prompt = new Prompt { Name = "One", Description = string.Empty, Arguments = [] };
        var clientPrompt = new McpClientPrompt(mcpClient, prompt);

        var entry = new McpServerState.ServerEntry
        {
            Enabled = true,
            Status = ServerStatus.Ready,
            Prompts = new[] { clientPrompt }
        };
        var dict = new ConcurrentDictionary<string, McpServerState.ServerEntry>(StringComparer.OrdinalIgnoreCase)
        {
            ["server"] = entry
        };
        var state = new McpServerState(dict);
        var manager = new McpServerManager(state);
        var prompts = new McpPromptCollection(manager);

        var chatClient = new FakeChatClient { Response = new(new ChatMessage(ChatRole.Assistant, "One")) };
        var strategy = new SuggestPromptsCommandStrategy(chatClient, prompts);
        var history = new ChatHistoryService();
        var console = Substitute.For<IChatConsole>();

        // Act
        await strategy.ExecuteAsync("/suggest", history, Substitute.For<IChatController>(), console);

        // Assert
        console.Received(1).DisplayError(expectedException);
    }
}
