using RaindropServer.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace RaindropServer.Tests;

public class FiltersTests : TestBase
{
    public FiltersTests() : base(s => s.AddTransient<FiltersTools>()) { }

    [Fact(Skip = "Requires live Raindrop API")]
    public async Task List()
    {
        var filters = Provider.GetRequiredService<FiltersTools>();
        var response = await filters.GetAvailableFiltersAsync(0);
        Assert.True(response.Result);
    }
}
