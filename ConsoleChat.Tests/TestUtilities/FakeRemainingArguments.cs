using Spectre.Console.Cli;
using System.Collections.Generic;
using System.Linq;

namespace ConsoleChat.Tests.TestUtilities;

/// <summary>
/// Minimal <see cref="IRemainingArguments"/> implementation used by tests.
/// </summary>
public sealed class FakeRemainingArguments : IRemainingArguments
{
    public ILookup<string, string?> Parsed { get; } =
        Array.Empty<(string, string?)>().ToLookup(t => t.Item1, t => t.Item2);
    public IReadOnlyList<string> Raw { get; } = Array.Empty<string>();
}
