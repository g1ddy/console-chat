using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RadLine;
using SemanticKernelChat.Console;
using Xunit;
using NSubstitute;
using Spectre.Console.Testing;

namespace ConsoleChat.Tests;

public class ChatLineEditorTests
{
    [Fact]
    public async Task ReadLine_ShouldLoadHistoryOnce()
    {
        // Arrange
        var completion = Substitute.For<ITextCompletion>();
        var testConsole = new TestConsole();

        var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var safeDir = Path.Combine(userProfile, ".config/semantickernelchat");
        Directory.CreateDirectory(safeDir);
        var historyFile = Path.Combine(safeDir, "test.history");
        await File.WriteAllLinesAsync(historyFile, new[] { "line1", "line2" });

        string envVarName = "CHAT_HISTORY_FILE";
        string? originalValue = Environment.GetEnvironmentVariable(envVarName);

        try
        {
            Environment.SetEnvironmentVariable(envVarName, historyFile);
            var editor = new ChatLineEditor(completion, testConsole);

            // Assert before: history should be empty before first ReadLine
            Assert.Equal(0, editor._editor.History.Count);

            // Let's use reflection to await the private _historyLoader
            var loaderField = typeof(ChatLineEditor).GetField("_historyLoader", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var loader = (Lazy<Task>)loaderField!.GetValue(editor)!;
            await loader.Value;

            // Assert after loading
            Assert.Equal(2, editor._editor.History.Count);
        }
        finally
        {
            Environment.SetEnvironmentVariable(envVarName, originalValue);
            if (File.Exists(historyFile)) File.Delete(historyFile);
        }
    }
}
