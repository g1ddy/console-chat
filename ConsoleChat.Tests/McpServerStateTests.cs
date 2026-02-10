using System.Collections.Concurrent;
using System.Linq;
using System.Text.Json;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using SemanticKernelChat.Infrastructure;
using Xunit;

namespace ConsoleChat.Tests;

public class McpServerStateTests
{
    private McpServerState CreateState(params (string name, bool enabled, ServerStatus status)[] servers)
    {
        var dict = new ConcurrentDictionary<string, McpServerState.ServerEntry>(StringComparer.OrdinalIgnoreCase);
        foreach (var (name, enabled, status) in servers)
        {
            dict[name] = new McpServerState.ServerEntry
            {
                Enabled = enabled,
                Status = status
            };
        }
        return new McpServerState(dict);
    }

    private McpClientTool CreateTool(string name)
    {
        // Passing null for McpClient as we don't need it for these tests
        // and instantiating it is difficult due to internal/protected constructors.
        // We try to use NSubstitute to create a mock.
        var client = NSubstitute.Substitute.For<McpClient>();
        return new McpClientTool(client, new Tool { Name = name }, null);
    }

    private McpClientPrompt CreatePrompt(string name)
    {
        // Passing null for McpClient as we don't need it for these tests.
        var client = NSubstitute.Substitute.For<McpClient>();
        return new McpClientPrompt(client, new Prompt { Name = name });
    }

    [Fact]
    public void Constructor_InitializesEmpty()
    {
        var state = new McpServerState();
        Assert.Empty(state.Servers);
    }

    [Fact]
    public void Constructor_InitializesWithServers()
    {
        var state = CreateState(("server1", true, ServerStatus.Ready));
        Assert.Single(state.Servers);
        Assert.Contains("server1", state.Servers);
    }

    [Fact]
    public void GetEntry_ReturnsEntry_WhenExists()
    {
        var state = CreateState(("server1", true, ServerStatus.Ready));
        var entry = state.GetEntry("server1");
        Assert.NotNull(entry);
        Assert.True(entry!.Enabled);
        Assert.Equal(ServerStatus.Ready, entry.Status);
    }

    [Fact]
    public void GetEntry_ReturnsNull_WhenNotExists()
    {
        var state = new McpServerState();
        var entry = state.GetEntry("server1");
        Assert.Null(entry);
    }

    [Fact]
    public void IsServerEnabled_ReturnsTrue_WhenEnabled()
    {
        var state = CreateState(("server1", true, ServerStatus.Ready));
        Assert.True(state.IsServerEnabled("server1"));
    }

    [Fact]
    public void IsServerEnabled_ReturnsFalse_WhenDisabled()
    {
        var state = CreateState(("server1", false, ServerStatus.Ready));
        Assert.False(state.IsServerEnabled("server1"));
    }

    [Fact]
    public void SetServerEnabled_UpdatesStatus()
    {
        var state = CreateState(("server1", true, ServerStatus.Ready));
        state.SetServerEnabled("server1", false);
        Assert.False(state.IsServerEnabled("server1"));
    }

    [Fact]
    public void UpdateServerStatus_UpdatesStatus()
    {
        var state = CreateState(("server1", true, ServerStatus.None));
        state.UpdateServerStatus("server1", ServerStatus.Ready);
        var entry = state.GetEntry("server1");
        Assert.Equal(ServerStatus.Ready, entry!.Status);
    }

    [Fact]
    public void UpdateServerToolsAndPrompts_UpdatesToolsAndPrompts()
    {
        var state = CreateState(("server1", true, ServerStatus.Ready));
        var tools = new List<McpClientTool> { CreateTool("tool1") };
        var prompts = new List<McpClientPrompt> { CreatePrompt("prompt1") };

        state.UpdateServerToolsAndPrompts("server1", tools, prompts);

        var entry = state.GetEntry("server1");
        Assert.Equal(tools, entry!.Tools);
        Assert.Equal(prompts, entry.Prompts);
    }

    [Fact]
    public void GetTools_ReturnsOnlyEnabledAndReadyTools()
    {
        var state = CreateState(
            ("server1", true, ServerStatus.Ready),
            ("server2", false, ServerStatus.Ready),
            ("server3", true, ServerStatus.Loading)
        );

        var tool1 = CreateTool("tool1");
        var tool2 = CreateTool("tool2");
        var tool3 = CreateTool("tool3");

        state.UpdateServerToolsAndPrompts("server1", new[] { tool1 }, Array.Empty<McpClientPrompt>());
        state.UpdateServerToolsAndPrompts("server2", new[] { tool2 }, Array.Empty<McpClientPrompt>());
        state.UpdateServerToolsAndPrompts("server3", new[] { tool3 }, Array.Empty<McpClientPrompt>());

        var tools = state.GetTools();

        Assert.Single(tools);
        Assert.Equal("tool1", tools[0].Name);
    }

    [Fact]
    public void GetPrompts_ReturnsOnlyEnabledAndReadyPrompts()
    {
        var state = CreateState(
            ("server1", true, ServerStatus.Ready),
            ("server2", false, ServerStatus.Ready),
            ("server3", true, ServerStatus.Loading)
        );

        var prompt1 = CreatePrompt("prompt1");
        var prompt2 = CreatePrompt("prompt2");
        var prompt3 = CreatePrompt("prompt3");

        state.UpdateServerToolsAndPrompts("server1", Array.Empty<McpClientTool>(), new[] { prompt1 });
        state.UpdateServerToolsAndPrompts("server2", Array.Empty<McpClientTool>(), new[] { prompt2 });
        state.UpdateServerToolsAndPrompts("server3", Array.Empty<McpClientTool>(), new[] { prompt3 });

        var prompts = state.GetPrompts();

        Assert.Single(prompts);
        Assert.Equal("prompt1", prompts[0].Name);
    }

    [Fact]
    public void GetTools_CachesResult()
    {
        var state = CreateState(("server1", true, ServerStatus.Ready));
        var tool1 = CreateTool("tool1");
        state.UpdateServerToolsAndPrompts("server1", new[] { tool1 }, Array.Empty<McpClientPrompt>());

        var tools1 = state.GetTools();
        var tools2 = state.GetTools();

        Assert.Same(tools1, tools2);
    }

    [Fact]
    public void GetPrompts_CachesResult()
    {
        var state = CreateState(("server1", true, ServerStatus.Ready));
        var prompt1 = CreatePrompt("prompt1");
        state.UpdateServerToolsAndPrompts("server1", Array.Empty<McpClientTool>(), new[] { prompt1 });

        var prompts1 = state.GetPrompts();
        var prompts2 = state.GetPrompts();

        Assert.Same(prompts1, prompts2);
    }

    [Fact]
    public void GetTools_InvalidatesCache_OnUpdate()
    {
        var state = CreateState(("server1", true, ServerStatus.Ready));
        var tool1 = CreateTool("tool1");
        state.UpdateServerToolsAndPrompts("server1", new[] { tool1 }, Array.Empty<McpClientPrompt>());

        var tools1 = state.GetTools();

        // Update with new tool
        var tool2 = CreateTool("tool2");
        state.UpdateServerToolsAndPrompts("server1", new[] { tool1, tool2 }, Array.Empty<McpClientPrompt>());

        var tools2 = state.GetTools();

        Assert.NotSame(tools1, tools2);
        Assert.Equal(2, tools2.Count);
    }

    [Fact]
    public void GetServerInfos_ReturnsCorrectInfo()
    {
        var state = CreateState(
            ("server1", true, ServerStatus.Ready),
            ("server2", false, ServerStatus.Ready)
        );

        var tool1 = CreateTool("tool1");
        state.UpdateServerToolsAndPrompts("server1", new[] { tool1 }, Array.Empty<McpClientPrompt>());

        var infos = state.GetServerInfos();

        Assert.Equal(2, infos.Count);

        var server1 = infos.First(i => i.Name == "server1");
        Assert.True(server1.Enabled);
        Assert.Equal(ServerStatus.Ready, server1.Status);
        Assert.Single(server1.Tools);

        var server2 = infos.First(i => i.Name == "server2");
        Assert.False(server2.Enabled);
        Assert.Equal(ServerStatus.Ready, server2.Status);
        Assert.Empty(server2.Tools);
    }

    [Fact]
    public void GetPromptInfos_ReturnsCorrectInfo()
    {
        var state = CreateState(
            ("server1", true, ServerStatus.Ready),
            ("server2", false, ServerStatus.Ready)
        );

        var prompt1 = CreatePrompt("prompt1");
        state.UpdateServerToolsAndPrompts("server1", Array.Empty<McpClientTool>(), new[] { prompt1 });

        var infos = state.GetPromptInfos();

        Assert.Equal(2, infos.Count);

        var server1 = infos.First(i => i.Name == "server1");
        Assert.True(server1.Enabled);
        Assert.Equal(ServerStatus.Ready, server1.Status);
        Assert.Single(server1.Prompts);

        var server2 = infos.First(i => i.Name == "server2");
        Assert.False(server2.Enabled);
        Assert.Equal(ServerStatus.Ready, server2.Status);
        Assert.Empty(server2.Prompts);
    }
}
