using SemanticKernelChat.Infrastructure;
using System.Linq;

namespace SemanticKernelChat.Console;

internal sealed class ListPromptsCommandStrategy : ListCommandStrategyBase<McpServerState.McpPromptInfo>
{
    public const string Command = CliConstants.Commands.List;
    private readonly McpPromptCollection _prompts;

    public ListPromptsCommandStrategy(McpPromptCollection prompts)
        : base(CliConstants.Options.Prompts)
    {
        _prompts = prompts;
    }

    protected override IEnumerable<McpServerState.McpPromptInfo> GetServerInfos() => _prompts.GetServerInfos();

    protected override bool IsEnabled(McpServerState.McpPromptInfo info) => info.Enabled;

    protected override string GetName(McpServerState.McpPromptInfo info) => info.Name;

    protected override ServerStatus GetStatus(McpServerState.McpPromptInfo info) => info.Status;

    protected override IEnumerable<string> GetItemNames(McpServerState.McpPromptInfo info) => info.Prompts.Select(p => p.Name);
}
