using ConsoleChat.Tests.TestUtilities;
using Microsoft.Extensions.AI;
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
}
