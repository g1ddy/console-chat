using System;

namespace SemanticKernelChat.Console;

internal static class StringExtensions
{
    public static string[] SplitCommandArguments(this string? input)
        => input?.Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
}
