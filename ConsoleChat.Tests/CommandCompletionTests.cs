using NSubstitute;
using SemanticKernelChat.Console;

namespace ConsoleChat.Tests;

public class CommandCompletionTests
{
    [Fact]
    public void GetCompletions_ReturnsNull_WhenNoStrategies()
    {
        // Arrange
        var completion = new CommandCompletion(Enumerable.Empty<IChatCommandStrategy>());

        // Act
        var result = completion.GetCompletions("", "", "");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetCompletions_ReturnsNull_WhenStrategiesReturnNullOrEmpty()
    {
        // Arrange
        var strategy1 = Substitute.For<IChatCommandStrategy>();
        strategy1.GetCompletions(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).Returns((IEnumerable<string>?)null);

        var strategy2 = Substitute.For<IChatCommandStrategy>();
        strategy2.GetCompletions(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).Returns(Enumerable.Empty<string>());

        var completion = new CommandCompletion(new[] { strategy1, strategy2 });

        // Act
        var result = completion.GetCompletions("", "", "");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetCompletions_AggregatesCompletions_FromMultipleStrategies()
    {
        // Arrange
        var strategy1 = Substitute.For<IChatCommandStrategy>();
        strategy1.GetCompletions("pre", "wo", "suf").Returns(new[] { "apple", "banana" });

        var strategy2 = Substitute.For<IChatCommandStrategy>();
        strategy2.GetCompletions("pre", "wo", "suf").Returns(new[] { "cherry" });

        var completion = new CommandCompletion(new[] { strategy1, strategy2 });

        // Act
        var result = completion.GetCompletions("pre", "wo", "suf");

        // Assert
        Assert.NotNull(result);
        var list = result.ToList();
        Assert.Equal(3, list.Count);
        Assert.Contains("apple", list);
        Assert.Contains("banana", list);
        Assert.Contains("cherry", list);
    }

    [Fact]
    public void GetCompletions_DeduplicatesCompletions_CaseInsensitive()
    {
        // Arrange
        var strategy1 = Substitute.For<IChatCommandStrategy>();
        strategy1.GetCompletions(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).Returns(new[] { "apple", "Banana" });

        var strategy2 = Substitute.For<IChatCommandStrategy>();
        strategy2.GetCompletions(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).Returns(new[] { "APPLE", "cherry" });

        var completion = new CommandCompletion(new[] { strategy1, strategy2 });

        // Act
        var result = completion.GetCompletions("", "", "");

        // Assert
        Assert.NotNull(result);
        var list = result.ToList();
        Assert.Equal(3, list.Count);
        Assert.Contains("apple", list, StringComparer.OrdinalIgnoreCase);
        Assert.Contains("Banana", list, StringComparer.OrdinalIgnoreCase);
        Assert.Contains("cherry", list, StringComparer.OrdinalIgnoreCase);

        // Specifically check that we don't have both "apple" and "APPLE"
        Assert.Single(list, x => x.Equals("apple", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void GetCompletions_HandlesMixedNullAndValidResults()
    {
        // Arrange
        var strategy1 = Substitute.For<IChatCommandStrategy>();
        strategy1.GetCompletions(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).Returns((IEnumerable<string>?)null);

        var strategy2 = Substitute.For<IChatCommandStrategy>();
        strategy2.GetCompletions(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).Returns(new[] { "apple" });

        var completion = new CommandCompletion(new[] { strategy1, strategy2 });

        // Act
        var result = completion.GetCompletions("", "", "");

        // Assert
        Assert.NotNull(result);
        var list = result.ToList();
        Assert.Single(list);
        Assert.Equal("apple", list[0]);
    }
}
