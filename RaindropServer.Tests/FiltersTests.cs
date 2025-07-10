using RaindropServer.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace RaindropServer.Tests;

public class FiltersTests : TestBase
{
    public FiltersTests() : base(s => s.AddTransient<FiltersTools>()) { }

    [Fact(Skip="Requires live Raindrop API")]
    public async Task Crud()
    {
        var filters = Provider.GetRequiredService<FiltersTools>();
        var createResponse = await filters.CreateFilterAsync(new Filter
        {
            Title = "Filters Crud - Create",
            Query = "created:true"
        });
        int filterId = createResponse.Item.Id;
        try
        {
            await filters.UpdateFilterAsync(filterId, new Filter
            {
                Title = "Filters Crud - Updated",
                Query = "created:false"
            });
            var list = await filters.ListFiltersAsync();
            Assert.Contains(list.Items, f => f.Id == filterId);
            var retrieved = await filters.GetFilterAsync(filterId);
            Assert.Equal("Filters Crud - Updated", retrieved.Item.Title);
        }
        finally
        {
            await filters.DeleteFilterAsync(filterId);
        }
    }
}
