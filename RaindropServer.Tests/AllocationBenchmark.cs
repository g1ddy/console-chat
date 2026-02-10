using System;
using System.Collections.Generic;
using System.Linq;
using RaindropServer.Tags;
using Xunit;
using Xunit.Abstractions;

namespace RaindropServer.Tests;

public class AllocationBenchmark
{
    private readonly ITestOutputHelper _output;

    public AllocationBenchmark(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void Benchmark_RenameTagsAsync_Allocation_Optimized()
    {
        // Setup
        var inputTags = Enumerable.Range(0, 1000).Select(i => $"tag_{i}").ToList();
        var newTag = "new_tag";
        IEnumerable<string> inputEnumerable = inputTags;

        // Warmup
        var warmup = new TagRenameRequest { Replace = newTag, Tags = inputEnumerable };

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        long startBytes = GC.GetAllocatedBytesForCurrentThread();

        const int iterations = 1000;
        for (int i = 0; i < iterations; i++)
        {
            // Simulate NEW behavior: direct assignment
            var payload = new TagRenameRequest { Replace = newTag, Tags = inputEnumerable };
        }

        long endBytes = GC.GetAllocatedBytesForCurrentThread();
        long totalBytes = endBytes - startBytes;

        _output.WriteLine($"[OPTIMIZED_RENAME] Total allocated bytes over {iterations} iterations: {totalBytes:N0}");
        _output.WriteLine($"[OPTIMIZED_RENAME] Average bytes per iteration: {totalBytes / (double)iterations:N2}");

        Assert.True(totalBytes < 100 * iterations, "Allocation per iteration should be less than 100 bytes");
    }

    [Fact]
    public void Benchmark_DeleteTagsAsync_Allocation_Optimized()
    {
        // Setup
        var inputTags = Enumerable.Range(0, 1000).Select(i => $"tag_{i}").ToList();
        IEnumerable<string> inputEnumerable = inputTags;

        // Warmup
        var warmup = new TagDeleteRequest { Tags = inputEnumerable };

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        long startBytes = GC.GetAllocatedBytesForCurrentThread();

        const int iterations = 1000;
        for (int i = 0; i < iterations; i++)
        {
            // Simulate NEW behavior: direct assignment
            var payload = new TagDeleteRequest { Tags = inputEnumerable };
        }

        long endBytes = GC.GetAllocatedBytesForCurrentThread();
        long totalBytes = endBytes - startBytes;

        _output.WriteLine($"[OPTIMIZED_DELETE] Total allocated bytes over {iterations} iterations: {totalBytes:N0}");
        _output.WriteLine($"[OPTIMIZED_DELETE] Average bytes per iteration: {totalBytes / (double)iterations:N2}");

        Assert.True(totalBytes < 100 * iterations, "Allocation per iteration should be less than 100 bytes");
    }
}
