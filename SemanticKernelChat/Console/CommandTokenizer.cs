namespace SemanticKernelChat.Console;

internal static class CommandTokenizer
{
    public static string[] SplitArguments(string input)
        => input.Split(' ', StringSplitOptions.TrimEntries);
}
