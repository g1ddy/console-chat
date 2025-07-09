using RaindropServer.Raindrops;
using RaindropServer.Tags;
using Microsoft.Extensions.DependencyInjection;

namespace RaindropServer.Tests;

public class TagsTests : TestBase
{
    public TagsTests() : base(s =>
    {
        s.AddTransient<RaindropsTools>();
        s.AddTransient<TagsTools>();
    }) { }

    [Fact(Skip="Requires live Raindrop API")]
    public async Task Crud()
    {
        var raindropsTool = Provider.GetRequiredService<RaindropsTools>();
        var createResponse = await raindropsTool.CreateBookmarkAsync(null, "https://example.com/tag", "Tags Crud - Raindrop", tags: [ "TagRenameTestOne" ]);
        long raindropId = createResponse.Item.Id;
        var tagsTool = Provider.GetRequiredService<TagsTools>();
        try
        {
            await tagsTool.RenameTagAsync("TagRenameTestOne", "TagRenameTestTwo");
            var list = await tagsTool.ListTagsAsync();
            Assert.Contains(list.Items, t => t.Id == "TagRenameTestTwo");
        }
        finally
        {
            await tagsTool.DeleteTagAsync("TagRenameTestTwo");
            await raindropsTool.DeleteBookmarkAsync(raindropId);
        }
    }

    [Fact(Skip="Requires live Raindrop API")]
    public async Task CrudForCollection()
    {
        var raindropsTool = Provider.GetRequiredService<RaindropsTools>();
        var createResponse = await raindropsTool.CreateBookmarkAsync(null, "https://example.com/tag/collection", "Tags CrudForCollection - Raindrop", tags: [ "TagCollectionTestOne" ]);
        long raindropId = createResponse.Item.Id;
        var tagsTool = Provider.GetRequiredService<TagsTools>();
        try
        {
            await tagsTool.RenameTagAsync("TagCollectionTestOne", "TagCollectionTestTwo", 0);
            var list = await tagsTool.ListTagsAsync();
            Assert.Contains(list.Items, t => t.Id == "TagCollectionTestTwo");
            await tagsTool.DeleteTagAsync("TagCollectionTestTwo", 0);
            var finalList = await tagsTool.ListTagsAsync();
            Assert.DoesNotContain(finalList.Items, t => t.Id == "TagCollectionTestTwo");
        }
        finally
        {
            await raindropsTool.DeleteBookmarkAsync(raindropId);
        }
    }
}
