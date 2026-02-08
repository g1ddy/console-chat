using Microsoft.Extensions.AI;
using SemanticKernelChat;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace ConsoleChat.Tests;

public class ChatHistoryReducerExtensionsTests
{
    [Fact]
    public void Extract_EmptyHistory_ReturnsEmpty()
    {
        // Arrange
        var history = new List<ChatMessage>();

        // Act
        var result = history.Extract(0);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void Extract_StartIndexOutOfBounds_ReturnsEmpty()
    {
        // Arrange
        var history = new List<ChatMessage>
        {
            new ChatMessage(ChatRole.User, "1"),
            new ChatMessage(ChatRole.Assistant, "2")
        };

        // Act
        var result = history.Extract(5);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void Extract_WithSystemMessage_IncludesSystemMessage()
    {
        // Arrange
        var history = new List<ChatMessage>
        {
            new ChatMessage(ChatRole.User, "1"),
            new ChatMessage(ChatRole.Assistant, "2")
        };
        var systemMessage = new ChatMessage(ChatRole.System, "System");

        // Act
        var result = history.Extract(0, systemMessage: systemMessage);

        // Assert
        Assert.Collection(result,
            item => Assert.Equal(systemMessage, item),
            item => Assert.Equal(history[0], item),
            item => Assert.Equal(history[1], item));
    }

    [Fact]
    public void Extract_WithStartIndex_ReturnsSubset()
    {
        // Arrange
        var history = new List<ChatMessage>
        {
            new ChatMessage(ChatRole.User, "1"),
            new ChatMessage(ChatRole.Assistant, "2"),
            new ChatMessage(ChatRole.User, "3")
        };

        // Act
        var result = history.Extract(1);

        // Assert
        Assert.Collection(result,
            item => Assert.Equal(history[1], item),
            item => Assert.Equal(history[2], item));
    }

    [Fact]
    public void Extract_WithFinalIndex_ReturnsSubset()
    {
        // Arrange
        var history = new List<ChatMessage>
        {
            new ChatMessage(ChatRole.User, "1"),
            new ChatMessage(ChatRole.Assistant, "2"),
            new ChatMessage(ChatRole.User, "3")
        };

        // Act
        var result = history.Extract(0, finalIndex: 1);

        // Assert
        Assert.Collection(result,
            item => Assert.Equal(history[0], item),
            item => Assert.Equal(history[1], item));
    }

    [Fact]
    public void Extract_WithFilter_FiltersMessages()
    {
        // Arrange
        var history = new List<ChatMessage>
        {
            new ChatMessage(ChatRole.User, "1"),
            new ChatMessage(ChatRole.Assistant, "skip"),
            new ChatMessage(ChatRole.User, "3")
        };

        // Act
        var result = history.Extract(0, filter: msg => msg.Text == "skip");

        // Assert
        Assert.Collection(result,
            item => Assert.Equal(history[0], item),
            item => Assert.Equal(history[2], item));
    }

    [Fact]
    public void LocateSummarizationBoundary_EmptyHistory_ReturnsZero()
    {
        // Arrange
        var history = new List<ChatMessage>();

        // Act
        var result = history.LocateSummarizationBoundary("key");

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void LocateSummarizationBoundary_AllSummarized_ReturnsCount()
    {
        // Arrange
        var history = new List<ChatMessage>
        {
            new ChatMessage(ChatRole.User, "1") { AdditionalProperties = new() { ["key"] = true } },
            new ChatMessage(ChatRole.Assistant, "2") { AdditionalProperties = new() { ["key"] = true } }
        };

        // Act
        var result = history.LocateSummarizationBoundary("key");

        // Assert
        Assert.Equal(2, result);
    }

    [Fact]
    public void LocateSummarizationBoundary_MixedSummarized_ReturnsFirstUnsummarized()
    {
        // Arrange
        var history = new List<ChatMessage>
        {
            new ChatMessage(ChatRole.User, "1") { AdditionalProperties = new() { ["key"] = true } },
            new ChatMessage(ChatRole.Assistant, "2"),
            new ChatMessage(ChatRole.User, "3") { AdditionalProperties = new() { ["key"] = true } }
        };

        // Act
        var result = history.LocateSummarizationBoundary("key");

        // Assert
        Assert.Equal(1, result);
    }

    [Fact]
    public void LocateSummarizationBoundary_NoneSummarized_ReturnsZero()
    {
        // Arrange
        var history = new List<ChatMessage>
        {
            new ChatMessage(ChatRole.User, "1"),
            new ChatMessage(ChatRole.Assistant, "2")
        };

        // Act
        var result = history.LocateSummarizationBoundary("key");

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void LocateSafeReductionIndex_FindsUserMessage()
    {
        // Arrange
        var history = new List<ChatMessage>
        {
            new ChatMessage(ChatRole.User, "0"),
            new ChatMessage(ChatRole.Assistant, "1"),
            new ChatMessage(ChatRole.User, "2"),
            new ChatMessage(ChatRole.Assistant, "3")
        };

        // Act
        var result = history.LocateSafeReductionIndex(1, thresholdCount: 1);

        // Assert
        Assert.Equal(2, result);
    }

    [Fact]
    public void LocateSafeReductionIndex_ReturnsTargetIndex_WhenNoUserFound()
    {
        // Arrange
        var history = new List<ChatMessage>
        {
            new ChatMessage(ChatRole.User, "0"),
            new ChatMessage(ChatRole.Assistant, "1")
        };

        // Act
        var result = history.LocateSafeReductionIndex(1);

        // Assert
        Assert.Equal(1, result);
    }

    [Fact]
    public void LocateSafeReductionIndex_SkipsFunctionCalls()
    {
        // Arrange
        var history = new List<ChatMessage>
        {
            new ChatMessage(ChatRole.User, "0"),
            new ChatMessage(ChatRole.Assistant, new[] { new FunctionCallContent("f", "a") }),
            new ChatMessage(ChatRole.Tool, new[] { new FunctionResultContent("f", "r") }),
            new ChatMessage(ChatRole.Assistant, "3")
        };

        // Act
        var result = history.LocateSafeReductionIndex(2);

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void LocateSafeReductionIndex_RespectsOffset()
    {
        // Arrange
        var history = new List<ChatMessage>
        {
            new ChatMessage(ChatRole.User, "0"),
            new ChatMessage(ChatRole.Assistant, "1")
        };

        // Act
        var result = history.LocateSafeReductionIndex(1, offsetCount: 1);

        // Assert
        Assert.Equal(-1, result);
    }

    [Fact]
    public void LocateSafeReductionIndex_WithSystemMessage_AdjustsTarget()
    {
        // Arrange
        var history = new List<ChatMessage>
        {
            new ChatMessage(ChatRole.System, "sys"),
            new ChatMessage(ChatRole.User, "0"),
            new ChatMessage(ChatRole.Assistant, "1")
        };

        // Act
        var result = history.LocateSafeReductionIndex(2, thresholdCount: 1, hasSystemMessage: true);

        // Assert
        Assert.Equal(1, result);
    }
}
