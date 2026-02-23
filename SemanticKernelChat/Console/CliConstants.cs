namespace SemanticKernelChat.Console;

internal static class CliConstants
{
    public const string WelcomeMessage =
        "[grey]Welcome to ConsoleChat! Use Shift+Enter for a new line. Type '/exit' on an empty line to quit.[/]";

    public const string ExitMessage = "[red]Exiting ConsoleChat...[/]";
    public const string UserPrompt = "[grey]You: [/]";
    public const string ThinkingMessage = "Thinking...";
    public const string GenericErrorMessage = "An unexpected error occurred. Please try again.";
    public const string SummarizationPrompt = "Summarize the previous conversation in a concise form.";

    public class Commands
    {
        public static readonly HashSet<string> All = [
            Debug,
            Disable,
            Enable,
            Exit,
            Help,
            List,
            Toggle,
            Use,
            Summarize,
            Suggest,
        ];

        public const string Debug = "/debug";
        public const string Disable = "/disable";
        public const string Enable = "/enable";
        public const string Exit = "/exit";
        public const string Help = "/help";
        public const string List = "/list";
        public const string Toggle = "/toggle";
        public const string Use = "/use";
        public const string Summarize = "/summarize";
        public const string Suggest = "/suggest";
    }

    public static class Options
    {
        public const string Mcp = "mcp";
        public const string Tools = "tools";
        public const string Prompts = "prompts";
    }

    public static class MultiSelection
    {
        public const string Instructions = "[grey](Press <space> to toggle, <enter> to accept)[/]";
        public const string Enabled = "[green](enabled)[/]";
        public const string Disabled = "[red](disabled)[/]";
        public const string SelectedHeader = "[bold]Selected[/]";
        public const string SelectionMarker = "[yellow]*[/]";
        public const string NoSelections = "[grey]No selections made.[/]";
    }

    public static class Tool
    {
        public const string ParametersFormat = "[grey]:wrench: {0} Parameters...[/]";
        public const string ResultFormat = "[grey]:wrench: {0} Result...[/]";
        public const string WarningFormat = "[orange1]:warning: {0}[/]";
    }

    public static class Roles
    {
        public const string UserHeader = ":bust_in_silhouette: User";
        public const string AssistantHeader = ":robot: Assistant";
        public const string ToolHeader = ":wrench: Tool";
    }
}
