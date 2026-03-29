using System;
using System.IO;
using SemanticKernelChat.Console;
using Xunit;

namespace ConsoleChat.Tests;

public class ReproSecurityTests
{
    [Fact]
    public void IsPathSafe_ShouldRejectSymlinksInPath()
    {
        // Use a temporary directory for the entire test to ensure isolation
        var testWorkDir = Path.Combine(Path.GetTempPath(), "ChatLineEditorTests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(testWorkDir);

        try
        {
            var safeDir = Path.Combine(testWorkDir, "safe_dir");
            Directory.CreateDirectory(safeDir);

            // Create a "secret" file outside the safe directory
            var secretFile = Path.Combine(testWorkDir, "sensitive_data.txt");
            File.WriteAllText(secretFile, "this is secret");

            // Create a symlink inside the safe directory pointing to the work directory (parent of safe_dir)
            var symlinkDir = Path.Combine(safeDir, "traversal_link");

            try
            {
                // Create symlink: {testWorkDir}/safe_dir/traversal_link -> {testWorkDir}
                File.CreateSymbolicLink(symlinkDir, testWorkDir);
            }
            catch (UnauthorizedAccessException)
            {
                // Skip if current environment does not allow symlink creation (common on Windows without Dev Mode)
                return;
            }
            catch (IOException)
            {
                // Fallback for systems that might not support symlinks but might support junctions if we were on Windows
                // but for now, we'll just skip if link creation fails.
                return;
            }

            // Path that looks safe but uses a symlink to escape
            var maliciousPath = Path.Combine(symlinkDir, "sensitive_data.txt");

            // Act
            bool isSafe = ChatLineEditor.IsPathSafe(maliciousPath, safeDir, out var validatedPath);

            // Assert
            Assert.False(isSafe, "Vulnerability: Path containing intermediate symlink was accepted as safe.");
            Assert.Null(validatedPath);
        }
        finally
        {
            try
            {
                Directory.Delete(testWorkDir, true);
            }
            catch
            {
                // Best effort cleanup
            }
        }
    }

    [Fact]
    public void IsPathSafe_ShouldRejectPathOutsideSafeRoot()
    {
        var testWorkDir = Path.Combine(Path.GetTempPath(), "ChatLineEditorTests_Outside_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(testWorkDir);

        try
        {
            var safeDir = Path.Combine(testWorkDir, "safe_dir");
            Directory.CreateDirectory(safeDir);

            var outsideFile = Path.Combine(testWorkDir, "outside.txt");
            File.WriteAllText(outsideFile, "outside content");

            // Act
            bool isSafe = ChatLineEditor.IsPathSafe(outsideFile, safeDir, out var validatedPath);

            // Assert
            Assert.False(isSafe);
            Assert.Null(validatedPath);
        }
        finally
        {
            try
            {
                Directory.Delete(testWorkDir, true);
            }
            catch { }
        }
    }

    [Fact]
    public void IsPathSafe_ShouldAcceptValidPath()
    {
        var testWorkDir = Path.Combine(Path.GetTempPath(), "ChatLineEditorTests_Valid_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(testWorkDir);

        try
        {
            var safeDir = Path.Combine(testWorkDir, "safe_dir");
            Directory.CreateDirectory(safeDir);

            var validFile = Path.Combine(safeDir, "history.txt");
            // File doesn't need to exist for IsPathSafe to return true if it's just a path check,
            // but the loop uses FileInfo.Exists and Attributes for the reparse check.
            // If it doesn't exist, it's considered safe (no symlink yet).
            File.WriteAllText(validFile, "history content");

            // Act
            bool isSafe = ChatLineEditor.IsPathSafe(validFile, safeDir, out var validatedPath);

            // Assert
            Assert.True(isSafe);
            Assert.Equal(Path.GetFullPath(validFile), validatedPath);
        }
        finally
        {
            try
            {
                Directory.Delete(testWorkDir, true);
            }
            catch { }
        }
    }
}
