namespace SemanticKernelChat.Console;

internal static class CommandTokenizer
{
    public static string[] SplitArguments(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return Array.Empty<string>();
        }

        return input.Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
    }
}
