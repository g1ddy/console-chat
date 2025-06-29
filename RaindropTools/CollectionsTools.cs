using System.ComponentModel;
using System.Net.Http;
using ModelContextProtocol.Server;

namespace RaindropTools;

/// <summary>
/// Tools for managing Raindrop collections.
/// </summary>
[McpServerToolType]
public static class CollectionsTools
{
    [McpServerTool, Description("List all collections for the current user")]
    public static async Task<string> List(string token)
    {
        var response = await RaindropApiClient.SendAsync(HttpMethod.Get, "collections", token);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    [McpServerTool, Description("Get details for a collection by id")]
    public static async Task<string> Get(string token, int id)
    {
        var response = await RaindropApiClient.SendAsync(HttpMethod.Get, $"collection/{id}", token);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    [McpServerTool, Description("Create a new collection")]
    public static async Task<string> Create(string token, CollectionUpdate collection)
    {
        var response = await RaindropApiClient.SendAsync(HttpMethod.Post, "collection", token, collection);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    [McpServerTool, Description("Update an existing collection")]
    public static async Task<string> Update(string token, int id, CollectionUpdate collection)
    {
        var response = await RaindropApiClient.SendAsync(HttpMethod.Put, $"collection/{id}", token, collection);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    [McpServerTool, Description("Delete a collection")]
    public static async Task<string> Delete(string token, int id)
    {
        var response = await RaindropApiClient.SendAsync(HttpMethod.Delete, $"collection/{id}", token);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    [McpServerTool, Description("Update order of child collections")]
    public static async Task<string> UpdateChildren(string token, int parentId, ChildCollectionsUpdate update)
    {
        var response = await RaindropApiClient.SendAsync(HttpMethod.Put, $"collection/{parentId}/children", token, update);
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

