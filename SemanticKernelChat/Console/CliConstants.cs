namespace SemanticKernelChat.Console;

internal static class CliConstants
{
    public const string WelcomeMessage =
        "[grey]Welcome to ConsoleChat! Use Shift+Enter for a new line. Type '/exit' on an empty line to quit.[/]";

    public const string ExitMessage = "[red]Exiting ConsoleChat...[/]";
    public const string UserPrompt = "[grey]You: [/]";

    public class Commands
    {
        public static readonly HashSet<string> All = [
            Help,
            Exit,
            Enable,
            Disable,
            Toggle,
            List,
            Use,
        ];

        public const string Help = "/help";
        public const string Exit = "/exit";
        public const string Enable = "/enable";
        public const string Disable = "/disable";
        public const string Toggle = "/toggle";
        public const string List = "/list";
        public const string Use = "/use";
    }

    public static class Options
    {
        public const string Mcp = "mcp";
        public const string Tools = "tools";
        public const string Prompts = "prompts";
    }
}
