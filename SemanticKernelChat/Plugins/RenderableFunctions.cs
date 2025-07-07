using System.ComponentModel;
using Microsoft.SemanticKernel;
using Spectre.Console;
using Spectre.Console.Rendering;
using SemanticKernelChat.Console;

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

    [KernelFunction, Description("Displays a table of items and counts in the console.")]
    public string SampleTable(IReadOnlyList<ItemCount> items)
    {
        var table = new Table().RoundedBorder();
        table.AddColumn("Item");
        table.AddColumn("Count");

        foreach (var item in items)
        {
            table.AddRow(item.Name, item.Count.ToString());
        }

        _console.Write(table);
        return "Displayed fruit table";
    }

    public sealed record TreeItem(string Name, IReadOnlyList<TreeItem>? Children);

    [KernelFunction, Description("Displays a simple tree structure in the console.")]
    public string SampleTree(TreeItem root)
    {
        var tree = new Tree(root.Name);
        AddChildren(tree, root.Children);

        _console.Write(tree);
        return "Displayed tree";
    }

    private static void AddChildren(IHasTreeNodes parent, IReadOnlyList<TreeItem>? children)
    {
        if (children is null)
        {
            return;
        }

        foreach (var child in children)
        {
            var node = parent.AddNode(child.Name);
            AddChildren(node, child.Children);
        }
    }

    public sealed record ChartItem(string Name, int Value, Color Color);

    [KernelFunction, Description("Displays a bar chart of values in the console.")]
    public string SampleChart(IReadOnlyList<ChartItem> items)
    {
        var chart = new BarChart()
            .Width(40)
            .Label("[bold green]Fruit Sales[/]")
            .CenterLabel();

        foreach (var item in items)
        {
            chart.AddItem(item.Name, item.Value, item.Color);
        }

        _console.Write(chart);
        return "Displayed chart";
    }
}
