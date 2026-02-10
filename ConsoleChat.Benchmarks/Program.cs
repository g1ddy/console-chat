using BenchmarkDotNet.Running;
using ConsoleChat.Benchmarks;

public class Program
{
    public static void Main(string[] args)
    {
        BenchmarkRunner.Run<DisplayStreamingUpdatesBenchmark>();
    }
}
