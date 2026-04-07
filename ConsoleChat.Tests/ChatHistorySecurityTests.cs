using System;
using System.IO;
using System.Threading.Tasks;
using RadLine;
using SemanticKernelChat.Console;
using Xunit;
using NSubstitute;

namespace ConsoleChat.Tests;

[Collection("HistoryTests")]
public class ChatHistorySecurityTests
{
    [Fact]
    public void ChatLineEditor_Constructor_ShouldNotAccessFilesOutsideSafeDirectory()
    {
        // Arrange
        var completion = Substitute.For<ITextCompletion>();
        var tempFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "unauthorized_outside.history");

        // Attempt traversal from a likely "safe" root (like current directory) to the temp directory
        // Or just use an absolute path that is clearly outside where we'd want it.
        // For reproduction, we just want to see if it reads it.

        string envVarName = "CHAT_HISTORY_FILE";
        string? originalValue = Environment.GetEnvironmentVariable(envVarName);

        var testConsole = new Spectre.Console.Testing.TestConsole();
        try
        {
            Environment.SetEnvironmentVariable(envVarName, tempFile);

            // Act
            var editor = new ChatLineEditor(completion, testConsole);

            // Assert
            string output = testConsole.Output.Replace("\n", " ").Replace("\r", "");
            Assert.Contains("invalid or outside the safe directory", output, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            Environment.SetEnvironmentVariable(envVarName, originalValue);
        }
    }

    [Fact]
    public void ChatLineEditor_Constructor_ShouldRejectHiddenFiles()
    {
        // Arrange
        var completion = Substitute.For<ITextCompletion>();
        var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var safeDir = Path.Combine(userProfile, ".config/semantickernelchat");
        var hiddenFile = Path.Combine(safeDir, ".bashrc");

        string envVarName = "CHAT_HISTORY_FILE";
        string? originalValue = Environment.GetEnvironmentVariable(envVarName);

        var testConsole = new Spectre.Console.Testing.TestConsole();
        try
        {
            Environment.SetEnvironmentVariable(envVarName, hiddenFile);

            // Act
            var editor = new ChatLineEditor(completion, testConsole);

            // Assert
            string output = testConsole.Output.Replace("\n", " ").Replace("\r", "");
            Assert.Contains("invalid or outside the safe directory", output, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            Environment.SetEnvironmentVariable(envVarName, originalValue);
        }
    }
}
