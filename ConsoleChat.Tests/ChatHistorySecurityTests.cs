using System;
using System.IO;
using System.Threading.Tasks;
using RadLine;
using SemanticKernelChat.Console;
using Xunit;
using NSubstitute;

namespace ConsoleChat.Tests;

public class ChatHistorySecurityTests
{
    [Fact]
    public void ChatLineEditor_Constructor_ShouldNotAccessFilesOutsideSafeDirectory()
    {
        // Arrange
        var completion = Substitute.For<ITextCompletion>();
        var tempFile = Path.Combine(Path.GetTempPath(), "unauthorized.history");

        // Attempt traversal from a likely "safe" root (like current directory) to the temp directory
        // Or just use an absolute path that is clearly outside where we'd want it.
        // For reproduction, we just want to see if it reads it.

        string envVarName = "CHAT_HISTORY_FILE";
        string originalValue = Environment.GetEnvironmentVariable(envVarName);

        var testConsole = new Spectre.Console.Testing.TestConsole();
        try
        {
            Environment.SetEnvironmentVariable(envVarName, tempFile);

            // Act
            var editor = new ChatLineEditor(completion, testConsole);

            // Assert
            string output = testConsole.Output;
            Assert.Contains("Warning", output);
        }
        finally
        {
            Environment.SetEnvironmentVariable(envVarName, originalValue);
        }
    }
}
