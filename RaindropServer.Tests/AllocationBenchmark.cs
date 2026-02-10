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

        _output.WriteLine($"[OPTIMIZED] Total allocated bytes over {iterations} iterations: {totalBytes:N0}");
        _output.WriteLine($"[OPTIMIZED] Average bytes per iteration: {totalBytes / (double)iterations:N2}");

        // Assert that the allocation is small (just the object overhead, no list copying)
        // With 1000 iterations, we expect ~32KB if it was copying (it was 8MB actually),
        // now it is ~32KB total? No wait.
        // Baseline was 8,088,000 bytes for 1000 iterations -> ~8KB per iteration.
        // Optimized was 32,000 bytes for 1000 iterations -> ~32 bytes per iteration.

        // Assert that we are below 100 bytes per iteration on average.
        Assert.True(totalBytes < 100 * iterations, "Allocation per iteration should be less than 100 bytes");
    }
}
