using System.ComponentModel;
using Refit;
using ModelContextProtocol.Server;

namespace RaindropTools;

/// <summary>
/// Tools for managing Raindrop collections.
/// </summary>
[McpServerToolType]
public class CollectionsTools
{
    private readonly IRaindropApi _api;

    public CollectionsTools(IRaindropApi api)
    {
        _api = api;
    }

    [McpServerTool, Description("List all collections for the current user")]
    public Task<ItemsResponse<Collection>> List()
    {
        return _api.ListCollections();
    }

    [McpServerTool, Description("Get details for a collection by id")]
    public Task<ItemResponse<Collection>> Get(int id)
    {
        return _api.GetCollection(id);
    }

    [McpServerTool, Description("Create a new collection")]
    public Task<ItemResponse<Collection>> Create(CollectionUpdate collection)
    {
        return _api.CreateCollection(collection);
    }

    [McpServerTool, Description("Update an existing collection")]
    public Task<ItemResponse<Collection>> Update(int id, CollectionUpdate collection)
    {
        return _api.UpdateCollection(id, collection);
    }

    [McpServerTool, Description("Delete a collection")]
    public Task<SuccessResponse> Delete(int id)
    {
        return _api.DeleteCollection(id);
    }

    [McpServerTool, Description("Update order of child collections")]
    public Task<ItemResponse<Collection>> UpdateChildren(int parentId, ChildCollectionsUpdate update)
    {
        return _api.UpdateChildCollections(parentId, update);
    }
}

/// <summary>
/// Payload used when creating or updating a collection.
/// </summary>
public class CollectionUpdate
{
    /// <summary>
    /// Title displayed for the collection.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Identifier of the parent collection. Use <c>null</c> for a root collection.
    /// </summary>
    public int? ParentId { get; set; }

    /// <summary>
    /// Optional hexadecimal color (e.g. "#ff0000").
    /// </summary>
    public string? Color { get; set; }

    /// <summary>
    /// Cover image URL that represents the collection.
    /// </summary>
    public string? Cover { get; set; }

    /// <summary>
    /// Set to <c>true</c> to share the collection publicly.
    /// </summary>
    public bool? IsPublic { get; set; }
}

/// <summary>
/// Payload for updating the order of child collections.
/// </summary>
public class ChildCollectionsUpdate
{
    /// <summary>
    /// Identifiers of child collections sorted in the desired order.
    /// </summary>
    public int[] Children { get; set; } = Array.Empty<int>();
}

