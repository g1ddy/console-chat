using System.ComponentModel;
using Microsoft.SemanticKernel;
using SemanticKernelChat.Console;
using Spectre.Console;

namespace SemanticKernelChat.Plugins;

/// <summary>
/// Example functions that render Spectre.Console components directly.
/// </summary>
public sealed class RenderableFunctions
{
    private readonly IChatConsole _console;

    public RenderableFunctions(IChatConsole console)
    {
        _console = console;
    }

    public sealed record ItemCount(string Name, int Count);

    [KernelFunction(nameof(RenderTable)), Description("Displays a table of items and counts in the console.")]
    public string RenderTable(IReadOnlyList<ItemCount> items)
    {
        var table = new Table().RoundedBorder();
        _ = table.AddColumn("Item");
        _ = table.AddColumn("Count");

        foreach (var item in items)
        {
            _ = table.AddRow(item.Name, item.Count.ToString());
        }

        _console.WritePanel(table, "Items");
        return "Displayed item table";
    }

    public class TreeNode
    {
        public string Value { get; set; }
        public List<TreeNode> Children { get; set; }

        public bool IsLeaf => Children == null || Children.Count == 0;

        public TreeNode(string value)
        {
            Value = value;
            Children = new List<TreeNode>();
        }

        public void AddChild(TreeNode child)
            => Children.Add(child);
    }

    [KernelFunction(nameof(RenderTree)), Description("Displays a simple tree structure in the console.")]
    public string RenderTree(TreeNode root)
    {
        var tree = new Tree(root.Value);
        AddChildren(tree, root.Children);

        _console.WritePanel(tree, "Tree");
        return "Displayed tree";
    }

    private static void AddChildren(IHasTreeNodes parent, IReadOnlyList<TreeNode>? children)
    {
        if (children is null)
        {
            return;
        }

        foreach (var child in children)
        {
            var node = parent.AddNode(child.Value);
            AddChildren(node, child.Children);
        }
    }

    public sealed record ChartItem(string Name, int Value, Color Color);

    [KernelFunction(nameof(RenderChart)), Description("Displays a bar chart of values in the console.")]
    public string RenderChart(IReadOnlyList<ChartItem> items, [Description("The title of the chart")] string title)
    {
        var chart = new BarChart()
            .Width(40)
            .Label($"[bold green]{title.EscapeMarkup()}[/]")
            .CenterLabel();

        foreach (var item in items)
        {
            _ = chart.AddItem(item.Name, item.Value, item.Color);
        }

        _console.WritePanel(chart, "Chart");
        return "Displayed chart";
    }
}
