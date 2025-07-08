namespace SemanticKernelChat.Console;

internal static class CommandTokenizer
{
    public static string[] SplitArguments(string input) =>
        input.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
}
