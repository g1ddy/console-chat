namespace RaindropServer.Common;

/// <summary>
/// Centralized description of the Raindrop search syntax.
/// </summary>
public static class SearchSyntax
{
    /// <summary>
    /// Explanation of supported search operators for filtering bookmarks and filters.
    /// </summary>
    public const string Description = """
A string for filtering the results based on a powerful search syntax. You can combine multiple operators. Key operators include:
 * Keywords: apple iphone finds items containing those words in the title, description, domain, or page content.
 * Exact Phrase: "superman vs. batman" finds the exact phrase.
 * Exclusion: -word or -#tag excludes items containing that word or tag.
 * Tags: #tag or #"multi word tag" finds items with specific tags.
 * Logical OR: term1 term2 match:OR finds items with either term.
 * Date Filtering: created:>YYYY-MM-DD or lastUpdate:YYYY-MM-DD finds items created or updated before/after a specific date.
 * Field-Specific Search: title:word, excerpt:word, note:word, link:word searches within specific fields.
 * Type Filtering: type:article, type:video, type:image, type:document, type:audio.
 * Attribute Filters: ❤️ (favorites), file:true (uploaded files), notag:true (items without tags), cache.status:ready (items with a permanent copy), reminder:true (items with a reminder).
""";
}
