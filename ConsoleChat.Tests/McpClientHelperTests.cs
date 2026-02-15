using System.IO;
using System.Threading.Tasks;
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

    [Fact]
    public async Task CreateTransportAsync_Returns_StdioClientTransport_For_Stdio_Type()
    {
        // Arrange
        var config = new McpServerConfig
        {
            Command = "dotnet",
            TransportType = "stdio"
        };

        // Act
        var transport = await McpClientHelper.CreateTransportAsync("test-server", config);

        // Assert
        Assert.IsType<StdioClientTransport>(transport);
    }

    [Fact]
    public async Task CreateTransportAsync_Returns_HttpClientTransport_For_Sse_Type()
    {
        // Arrange
        var config = new McpServerConfig
        {
            Command = "http://localhost:8080/sse",
            TransportType = "sse"
        };

        // Act
        var transport = await McpClientHelper.CreateTransportAsync("test-server", config);

        // Assert
        Assert.IsType<HttpClientTransport>(transport);
    }

    [Fact]
    public async Task CreateTransportAsync_Defaults_To_Stdio_When_TransportType_Is_Null()
    {
        // Arrange
        var config = new McpServerConfig
        {
            Command = "dotnet",
            TransportType = null
        };

        // Act
        var transport = await McpClientHelper.CreateTransportAsync("test-server", config);

        // Assert
        Assert.IsType<StdioClientTransport>(transport);
    }
}
