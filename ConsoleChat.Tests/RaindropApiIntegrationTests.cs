using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RaindropTools;
using System.Text.Json;
using Xunit;

namespace ConsoleChat.Tests;

public class RaindropApiIntegrationTests
{
    private readonly IServiceProvider _provider;
    private readonly bool _enabled;

    public RaindropApiIntegrationTests()
    {
        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var services = new ServiceCollection();
        services.AddRaindropApiClient(config);
        services.AddTransient<CollectionsTools>();
        services.AddTransient<RaindropsTools>();
        services.AddTransient<HighlightsTools>();
        services.AddTransient<TagsTools>();
        services.AddTransient<UserTools>();
        _provider = services.BuildServiceProvider();

        _enabled = !string.IsNullOrWhiteSpace(
            _provider.GetRequiredService<IOptions<RaindropOptions>>().Value.ApiToken);
    }

    private static int ExtractCollectionId(string json)
    {
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.GetProperty("item").GetProperty("_id").GetInt32();
    }

    private static long ExtractBookmarkId(string json)
    {
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.GetProperty("item").GetProperty("_id").GetInt64();
    }

    private static long ExtractHighlightId(string json)
    {
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.GetProperty("item").GetProperty("_id").GetInt64();
    }

    [Fact]
    public async Task Collections_Crud()
    {
        if (!_enabled) return;
        var tools = _provider.GetRequiredService<CollectionsTools>();
        var create = await tools.Create(new CollectionUpdate { Title = "test" });
        int id = ExtractCollectionId(create);
        try
        {
            await tools.Update(id, new CollectionUpdate { Title = "updated" });
            var get = await tools.Get(id);
            Assert.Contains("updated", get);
        }
        finally
        {
            await tools.Delete(id);
        }
    }

    [Fact]
    public async Task Bookmarks_Crud()
    {
        if (!_enabled) return;
        var collections = _provider.GetRequiredService<CollectionsTools>();
        var colJson = await collections.Create(new CollectionUpdate { Title = "bm" });
        int colId = ExtractCollectionId(colJson);
        var bookmarks = _provider.GetRequiredService<RaindropsTools>();
        var createJson = await bookmarks.Create(colId, "https://example.com", "title");
        long bId = ExtractBookmarkId(createJson);
        try
        {
            await bookmarks.Update(bId, title: "upd");
            var get = await bookmarks.Get(bId);
            Assert.Contains("upd", get);
        }
        finally
        {
            await bookmarks.Delete(bId);
            await collections.Delete(colId);
        }
    }

    [Fact]
    public async Task Highlights_Crud()
    {
        if (!_enabled) return;
        var collections = _provider.GetRequiredService<CollectionsTools>();
        int colId = ExtractCollectionId(await collections.Create(new CollectionUpdate { Title = "hl" }));
        var bookmarks = _provider.GetRequiredService<RaindropsTools>();
        long bId = ExtractBookmarkId(await bookmarks.Create(colId, "https://example.com/hl", "h"));
        var highlights = _provider.GetRequiredService<HighlightsTools>();
        long hId = ExtractHighlightId(await highlights.Create(bId, "test"));
        try
        {
            await highlights.Update(hId, "upd");
            var list = await highlights.Get(bId);
            Assert.Contains("upd", list);
        }
        finally
        {
            await highlights.Delete(hId);
            await bookmarks.Delete(bId);
            await collections.Delete(colId);
        }
    }

    [Fact]
    public async Task Tags_Crud()
    {
        if (!_enabled) return;
        var collections = _provider.GetRequiredService<CollectionsTools>();
        int colId = ExtractCollectionId(await collections.Create(new CollectionUpdate { Title = "tg" }));
        var bookmarks = _provider.GetRequiredService<RaindropsTools>();
        long bId = ExtractBookmarkId(await bookmarks.Create(colId, "https://example.com/tag", "t", tags: [ "one" ]));
        var tags = _provider.GetRequiredService<TagsTools>();
        try
        {
            await tags.Rename("one", "two");
            var list = await tags.List();
            Assert.Contains("two", list);
        }
        finally
        {
            await tags.Delete("two");
            await bookmarks.Delete(bId);
            await collections.Delete(colId);
        }
    }

    [Fact]
    public async Task Hierarchy_Test()
    {
        if (!_enabled) return;
        var collections = _provider.GetRequiredService<CollectionsTools>();
        int rootId = ExtractCollectionId(await collections.Create(new CollectionUpdate { Title = "root" }));
        int childId = ExtractCollectionId(await collections.Create(new CollectionUpdate { Title = "child", ParentId = rootId }));

        var bookmarks = _provider.GetRequiredService<RaindropsTools>();
        long b1 = ExtractBookmarkId(await bookmarks.Create(rootId,
            "https://example.com/1", "b1", tags: ["t1"]));
        long b2 = ExtractBookmarkId(await bookmarks.Create(rootId,
            "https://example.com/2", "b2", tags: ["t2"]));

        var highlights = _provider.GetRequiredService<HighlightsTools>();
        var tags = _provider.GetRequiredService<TagsTools>();

        try
        {
            await highlights.Create(b1, "hl");
            await bookmarks.Update(b2, link: "https://example.com/updated", collectionId: childId);
            await tags.Rename("t2", "t22");
            await tags.List();
            await collections.UpdateChildren(rootId, new ChildCollectionsUpdate { Children = [ childId ] });
        }
        finally
        {
            await bookmarks.Delete(b1);
            await bookmarks.Delete(b2);
            await collections.Delete(childId);
            await collections.Delete(rootId);
        }
    }
}
