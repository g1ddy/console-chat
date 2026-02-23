using McpServer;
using Xunit;

namespace ConsoleChat.Tests;

public class UtilityToolsTests
{
    [Fact]
    public void CurrentTime_ReturnsValidIso8601String()
    {
        // Act
        var result = UtilityTools.CurrentTime();

        // Assert
        Assert.True(DateTimeOffset.TryParse(result, out _));
    }

    [Fact]
    public void ToUpper_ConvertsToUpperCase()
    {
        // Arrange
        var input = "hello world";
        var expected = "HELLO WORLD";

        // Act
        var result = UtilityTools.ToUpper(input);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ToUpper_HandlesEmptyString()
    {
        // Arrange
        var input = "";
        var expected = "";

        // Act
        var result = UtilityTools.ToUpper(input);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ToUpper_ThrowsOnNullInput()
    {
        Assert.Throws<ArgumentNullException>(() => UtilityTools.ToUpper(null!));
    }

    [Theory]
    [InlineData(1, 2, 3)]
    [InlineData(-1, -1, -2)]
    [InlineData(0, 0, 0)]
    [InlineData(int.MaxValue, -1, int.MaxValue - 1)]
    public void Add_ReturnsCorrectSum(int a, int b, int expected)
    {
        // Act
        var result = UtilityTools.Add(a, b);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Add_ThrowsOnOverflow()
    {
        Assert.Throws<OverflowException>(() => UtilityTools.Add(int.MaxValue, 1));
    }

    [Fact]
    public void Add_ThrowsOnUnderflow()
    {
        Assert.Throws<OverflowException>(() => UtilityTools.Add(int.MinValue, -1));
    }
}
