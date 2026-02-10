using BenchmarkDotNet.Attributes;
using McpServer;

namespace ConsoleChat.Benchmarks;

[MemoryDiagnoser]
public class EchoBenchmark
{
    private string _message;

    [Params(10, 100, 1000)]
    public int MessageLength { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _message = new string('a', MessageLength);
    }

    [Benchmark]
    public string ReverseEcho()
    {
        return EchoTools.ReverseEcho(_message);
    }
}
