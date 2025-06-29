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

    [KernelFunction, Description("Displays a table of fruit counts in the console.")]
    public string SampleTable()
    {
        var table = new Table().RoundedBorder();
        table.AddColumn("Fruit");
        table.AddColumn("Count");
        table.AddRow("Apples", "12");
        table.AddRow("Bananas", "7");
        _console.Write(table);
        return "Displayed fruit table";
    }

    [KernelFunction, Description("Displays a simple tree structure in the console.")]
    public string SampleTree()
    {
        var tree = new Tree("Root");
        tree.AddNode("Branch 1").AddNode("Leaf");
        tree.AddNode("Branch 2");
        _console.Write(tree);
        return "Displayed tree";
    }

    [KernelFunction, Description("Displays a bar chart of fruit sales in the console.")]
    public string SampleChart()
    {
        var chart = new BarChart()
            .Width(40)
            .Label("[bold green]Fruit Sales[/]")
            .CenterLabel();
        chart.AddItem("Apples", 12, Color.Red);
        chart.AddItem("Bananas", 7, Color.Yellow);
        _console.Write(chart);
        return "Displayed chart";
    }
}
