using System.ComponentModel;
using System.Net.Http;
using ModelContextProtocol.Server;

namespace RaindropTools;

/// <summary>
/// Tools for managing Raindrop collections.
/// </summary>
[McpServerToolType]
public class CollectionsTools
{
    private readonly RaindropApiClient _client;

    public CollectionsTools(RaindropApiClient client)
    {
        _client = client;
    }

    [McpServerTool, Description("List all collections for the current user")]
    public async Task<ItemsResponse<Collection>> List()
    {
        return await _client.SendAsync<ItemsResponse<Collection>>(HttpMethod.Get, "collections");
    }

    [McpServerTool, Description("Get details for a collection by id")]
    public async Task<ItemResponse<Collection>> Get(int id)
    {
        return await _client.SendAsync<ItemResponse<Collection>>(HttpMethod.Get, $"collection/{id}");
    }

    [McpServerTool, Description("Create a new collection")]
    public async Task<ItemResponse<Collection>> Create(CollectionUpdate collection)
    {
        return await _client.SendAsync<ItemResponse<Collection>>(HttpMethod.Post, "collection", collection);
    }

    [McpServerTool, Description("Update an existing collection")]
    public async Task<ItemResponse<Collection>> Update(int id, CollectionUpdate collection)
    {
        return await _client.SendAsync<ItemResponse<Collection>>(HttpMethod.Put, $"collection/{id}", collection);
    }

    [McpServerTool, Description("Delete a collection")]
    public async Task<SuccessResponse> Delete(int id)
    {
        return await _client.SendAsync<SuccessResponse>(HttpMethod.Delete, $"collection/{id}");
    }

    [McpServerTool, Description("Update order of child collections")]
    public async Task<ItemResponse<Collection>> UpdateChildren(int parentId, ChildCollectionsUpdate update)
    {
        return await _client.SendAsync<ItemResponse<Collection>>(HttpMethod.Put, $"collection/{parentId}/children", update);
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

