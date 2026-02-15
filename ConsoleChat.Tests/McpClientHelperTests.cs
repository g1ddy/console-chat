using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using SemanticKernelChat.Helpers;
using ModelContextProtocol.Client;
using Xunit;

namespace ConsoleChat.Tests;

public class McpClientHelperTests
{
    [Theory]
    [InlineData("dotnet")]
    [InlineData("nvx")]
    public void ResolveCommandPath_Returns_System_Command_Unchanged(string command)
    {
        string result = McpClientHelper.ResolveCommandPath(command);
        Assert.Equal(command, result);
    }

    [Fact]
    public void ResolveCommandPath_Converts_Relative_Path()
    {
        string rel = $".{Path.DirectorySeparatorChar}tools{Path.DirectorySeparatorChar}server";
        string expected = Path.GetFullPath(rel, AppContext.BaseDirectory);
        string result = McpClientHelper.ResolveCommandPath(rel);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ResolveCommandPath_Returns_Absolute_Path_Unchanged()
    {
        string abs = Path.GetFullPath("server.exe");
        string result = McpClientHelper.ResolveCommandPath(abs);
        Assert.Equal(abs, result);
    }

    [Fact]
    public void ResolveCommandPath_Handles_Alternate_Separator()
    {
        char alt = Path.AltDirectorySeparatorChar;
        string rel = $"folder{alt}server";
        string expected = Path.GetFullPath(rel, AppContext.BaseDirectory);
        string result = McpClientHelper.ResolveCommandPath(rel);
        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task CreateTransportAsync_Throws_InvalidOperationException_On_Invalid_TransportType()
    {
        // Arrange
        var config = new McpServerConfig
        {
            Command = "test",
            TransportType = "invalid-type"
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            McpClientHelper.CreateTransportAsync("test-server", config));
    }

    public static IEnumerable<object?[]> CreateTransportAsync_HappyPath_TestData()
    {
        yield return new object?[] { "stdio", "dotnet", typeof(StdioClientTransport) };
        yield return new object?[] { "sse", "http://localhost:8080/sse", typeof(HttpClientTransport) };
        yield return new object?[] { null, "dotnet", typeof(StdioClientTransport) };
    }

    [Theory]
    [MemberData(nameof(CreateTransportAsync_HappyPath_TestData))]
    public async Task CreateTransportAsync_Returns_Correct_Transport_For_Valid_Type(string? transportType, string command, Type expectedTransportType)
    {
        // Arrange
        var config = new McpServerConfig
        {
            Command = command,
            TransportType = transportType
        };

        // Act
        var transport = await McpClientHelper.CreateTransportAsync("test-server", config);

        // Assert
        Assert.IsType(expectedTransportType, transport);
    }
}
