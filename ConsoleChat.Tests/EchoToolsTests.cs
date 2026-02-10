using McpServer;
using Xunit;

namespace ConsoleChat.Tests;

public class EchoToolsTests
{
    [Fact]
    public void ReverseEcho_ReversesStringCorrectly()
    {
        // Arrange
        var input = "Hello, World!";
        var expected = "!dlroW ,olleH";

        // Act
        var result = EchoTools.ReverseEcho(input);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ReverseEcho_HandlesEmptyString()
    {
        // Arrange
        var input = "";
        var expected = "";

        // Act
        var result = EchoTools.ReverseEcho(input);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ReverseEcho_HandlesSingleCharacter()
    {
        // Arrange
        var input = "a";
        var expected = "a";

        // Act
        var result = EchoTools.ReverseEcho(input);

        // Assert
        Assert.Equal(expected, result);
    }
}
