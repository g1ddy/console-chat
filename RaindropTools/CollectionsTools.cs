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
    public async Task<string> List()
    {
        var response = await _client.SendAsync(HttpMethod.Get, "collections");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    [McpServerTool, Description("Get details for a collection by id")]
    public async Task<string> Get(int id)
    {
        var response = await _client.SendAsync(HttpMethod.Get, $"collection/{id}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    [McpServerTool, Description("Create a new collection")]
    public async Task<string> Create(CollectionUpdate collection)
    {
        var response = await _client.SendAsync(HttpMethod.Post, "collection", collection);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    [McpServerTool, Description("Update an existing collection")]
    public async Task<string> Update(int id, CollectionUpdate collection)
    {
        var response = await _client.SendAsync(HttpMethod.Put, $"collection/{id}", collection);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    [McpServerTool, Description("Delete a collection")]
    public async Task<string> Delete(int id)
    {
        var response = await _client.SendAsync(HttpMethod.Delete, $"collection/{id}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    [McpServerTool, Description("Update order of child collections")]
    public async Task<string> UpdateChildren(int parentId, ChildCollectionsUpdate update)
    {
        var response = await _client.SendAsync(HttpMethod.Put, $"collection/{parentId}/children", update);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
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

