using Microsoft.Extensions.AI;
using NSubstitute;
using SemanticKernelChat;

namespace ConsoleChat.Tests;

public class ChatHistorySummarizationReducerTests
{
    [Fact]
    public void Constructor_Throws_ArgumentNullException_When_ChatClient_Is_Null()
    {
        Assert.Throws<ArgumentNullException>(() => new ChatHistorySummarizationReducer(null!, 10));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_Throws_ArgumentOutOfRangeException_When_TargetCount_Is_Invalid(int targetCount)
    {
        var chatClient = Substitute.For<IChatClient>();
        Assert.Throws<ArgumentOutOfRangeException>(() => new ChatHistorySummarizationReducer(chatClient, targetCount));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_Throws_ArgumentOutOfRangeException_When_ThresholdCount_Is_Invalid(int thresholdCount)
    {
        var chatClient = Substitute.For<IChatClient>();
        Assert.Throws<ArgumentOutOfRangeException>(() => new ChatHistorySummarizationReducer(chatClient, 10, thresholdCount));
    }

    [Fact]
    public async Task ReduceAsync_Returns_Null_When_History_Count_Is_Low()
    {
        // Arrange
        var chatClient = Substitute.For<IChatClient>();
        var reducer = new ChatHistorySummarizationReducer(chatClient, targetCount: 10);
        var history = new List<ChatMessage>
        {
            new(ChatRole.User, "Hello"),
            new(ChatRole.Assistant, "Hi there")
        };

        // Act
        var result = await reducer.ReduceAsync(history);

        // Assert
        Assert.Null(result);
        await chatClient.DidNotReceiveWithAnyArgs().GetResponseAsync(Arg.Any<List<ChatMessage>>(), Arg.Any<ChatOptions>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ReduceAsync_Summarizes_When_History_Exceeds_Threshold()
    {
        // Arrange
        var chatClient = Substitute.For<IChatClient>();
        var summaryText = "Summary of conversation";
        var summaryResponse = new ChatResponse(new List<ChatMessage>
        {
            new(ChatRole.Assistant, summaryText)
        });

        chatClient.GetResponseAsync(Arg.Any<List<ChatMessage>>(), Arg.Any<ChatOptions>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(summaryResponse));

        var targetCount = 2;
        var reducer = new ChatHistorySummarizationReducer(chatClient, targetCount, thresholdCount: 1);

        // Create a history that exceeds the target count significantly to trigger reduction
        // LocateSafeReductionIndex logic: targetCount=2. History count=5.
        // It looks for a User message backwards from index (Count - targetCount) = 3.
        // Index 3 is User. So it will likely reduce up to index 3.
        var history = new List<ChatMessage>
        {
            new(ChatRole.User, "Message 1"),      // 0
            new(ChatRole.Assistant, "Response 1"), // 1
            new(ChatRole.User, "Message 2"),      // 2
            new(ChatRole.Assistant, "Response 2"), // 3
            new(ChatRole.User, "Message 3"),      // 4
        };

        // Act
        var result = await reducer.ReduceAsync(history);

        // Assert
        Assert.NotNull(result);
        var resultList = result!.ToList();

        // Expecting:
        // Summary message (Assistant)
        // Message 3 (User) - from index 4?
        // Let's trace carefully.
        // LocateSafeReductionIndex logic is complex.

        // Verify summary message contains metadata
        var summaryMessage = resultList.FirstOrDefault(m => m.AdditionalProperties?.ContainsKey(ChatHistorySummarizationReducer.SummaryMetadataKey) == true);
        Assert.NotNull(summaryMessage);
        Assert.Equal(summaryText, summaryMessage!.Text);

        // Verify chat client was called
        await chatClient.Received(1).GetResponseAsync(Arg.Any<List<ChatMessage>>(), Arg.Any<ChatOptions>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ReduceAsync_Preserves_System_Message()
    {
        // Arrange
        var chatClient = Substitute.For<IChatClient>();
        var summaryResponse = new ChatResponse(new List<ChatMessage>
        {
            new(ChatRole.Assistant, "Summary")
        });

        chatClient.GetResponseAsync(Arg.Any<List<ChatMessage>>(), Arg.Any<ChatOptions>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(summaryResponse));

        var reducer = new ChatHistorySummarizationReducer(chatClient, targetCount: 1, thresholdCount: 1);
        var history = new List<ChatMessage>
        {
            new(ChatRole.System, "System instruction"),
            new(ChatRole.User, "User 1"),
            new(ChatRole.Assistant, "Assistant 1"),
            new(ChatRole.User, "User 2"),
        };

        // Act
        var result = await reducer.ReduceAsync(history);

        // Assert
        Assert.NotNull(result);
        var resultList = result!.ToList();
        Assert.Equal(ChatRole.System, resultList[0].Role);
        Assert.Equal("System instruction", resultList[0].Text);
    }

    [Fact]
    public async Task ReduceAsync_Throws_When_FailOnError_Is_True_And_Summarization_Fails()
    {
        // Arrange
        var chatClient = Substitute.For<IChatClient>();
        chatClient.GetResponseAsync(Arg.Any<List<ChatMessage>>(), Arg.Any<ChatOptions>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<ChatResponse>(new Exception("Summarization error")));

        var reducer = new ChatHistorySummarizationReducer(chatClient, targetCount: 1, thresholdCount: 1)
        {
            FailOnError = true
        };
        var history = new List<ChatMessage>
        {
            new(ChatRole.User, "User 1"),
            new(ChatRole.Assistant, "Assistant 1"),
            new(ChatRole.User, "User 2"),
        };

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => reducer.ReduceAsync(history));
    }

    [Fact]
    public async Task ReduceAsync_Returns_Null_When_FailOnError_Is_False_And_Summarization_Fails()
    {
        // Arrange
        var chatClient = Substitute.For<IChatClient>();
        chatClient.GetResponseAsync(Arg.Any<List<ChatMessage>>(), Arg.Any<ChatOptions>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<ChatResponse>(new Exception("Summarization error")));

        var reducer = new ChatHistorySummarizationReducer(chatClient, targetCount: 1, thresholdCount: 1)
        {
            FailOnError = false
        };
        var history = new List<ChatMessage>
        {
            new(ChatRole.User, "User 1"),
            new(ChatRole.Assistant, "Assistant 1"),
            new(ChatRole.User, "User 2"),
        };

        // Act
        var result = await reducer.ReduceAsync(history);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Equals_And_GetHashCode_Work_Correctly()
    {
        var chatClient = Substitute.For<IChatClient>();
        var reducer1 = new ChatHistorySummarizationReducer(chatClient, targetCount: 10, thresholdCount: 5)
        {
            UseSingleSummary = true,
            SummarizationInstructions = "Prompt"
        };

        var reducer2 = new ChatHistorySummarizationReducer(chatClient, targetCount: 10, thresholdCount: 5)
        {
            UseSingleSummary = true,
            SummarizationInstructions = "Prompt"
        };

        var reducer3 = new ChatHistorySummarizationReducer(chatClient, targetCount: 20, thresholdCount: 5);

        Assert.Equal(reducer1, reducer2);
        Assert.Equal(reducer1.GetHashCode(), reducer2.GetHashCode());
        Assert.NotEqual(reducer1, reducer3);
    }
}
