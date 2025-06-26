using SemanticKernelChat.Infrastructure;
using System.Linq;

namespace SemanticKernelChat.Console;

internal sealed class ListToolsCommandStrategy : ListCommandStrategyBase<McpServerState.McpServerInfo>
{
    public const string Command = CliConstants.Commands.List;
    private readonly McpToolCollection _tools;

    public ListToolsCommandStrategy(McpToolCollection tools)
        : base(CliConstants.Options.Tools)
    {
        _tools = tools;
    }

    protected override IEnumerable<McpServerState.McpServerInfo> GetServerInfos() => _tools.GetServerInfos();

    protected override bool IsEnabled(McpServerState.McpServerInfo info) => info.Enabled;

    protected override string GetName(McpServerState.McpServerInfo info) => info.Name;

    protected override ServerStatus GetStatus(McpServerState.McpServerInfo info) => info.Status;

    protected override IEnumerable<string> GetItemNames(McpServerState.McpServerInfo info) => info.Tools.Select(t => t.Name);
}

