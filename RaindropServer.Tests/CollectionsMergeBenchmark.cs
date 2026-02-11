using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RaindropServer.Collections;
using RaindropServer.Common;
using Xunit;
using Xunit.Abstractions;
using Refit;

namespace RaindropServer.Tests;

public class CollectionsMergeBenchmark
{
    private readonly ITestOutputHelper _output;

    public CollectionsMergeBenchmark(ITestOutputHelper output)
    {
        _output = output;
    }

    private class MockCollectionsApi : ICollectionsApi
    {
        public Task<SuccessResponse> MergeAsync(CollectionsMergeRequest payload)
        {
            return Task.FromResult(new SuccessResponse(true));
        }

        // Implement other methods as throw since they won't be called
        public Task<ItemsResponse<Collection>> ListAsync() => throw new NotImplementedException();
        public Task<ItemResponse<Collection>> GetAsync(int id) => throw new NotImplementedException();
        public Task<ItemResponse<Collection>> CreateAsync(Collection collection) => throw new NotImplementedException();
        public Task<ItemResponse<Collection>> UpdateAsync(int id, Collection collection) => throw new NotImplementedException();
        public Task<SuccessResponse> DeleteAsync(int id) => throw new NotImplementedException();
        public Task<ItemsResponse<Collection>> ListChildrenAsync() => throw new NotImplementedException();
    }

    [Fact]
    public async Task Benchmark_MergeCollectionsAsync_HashSet()
    {
        // Setup
        var api = new MockCollectionsApi();
        var tools = new CollectionsTools(api);

        int count = 100000;
        var ids = Enumerable.Range(0, count).ToHashSet();
        int to = count + 1; // Not in the set

        // Warmup
        await tools.MergeCollectionsAsync(to, ids);

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        long startBytes = GC.GetAllocatedBytesForCurrentThread();
        var watch = System.Diagnostics.Stopwatch.StartNew();

        const int iterations = 5000;
        for (int i = 0; i < iterations; i++)
        {
            await tools.MergeCollectionsAsync(to, ids);
        }

        watch.Stop();
        long endBytes = GC.GetAllocatedBytesForCurrentThread();
        long totalBytes = endBytes - startBytes;

        _output.WriteLine($"[OPTIMIZED_MERGE] Total time over {iterations} iterations: {watch.ElapsedMilliseconds}ms");
        _output.WriteLine($"[OPTIMIZED_MERGE] Average time per iteration: {watch.ElapsedMilliseconds / (double)iterations:N4}ms");
        _output.WriteLine($"[OPTIMIZED_MERGE] Total allocated bytes: {totalBytes:N0}");
    }
}
