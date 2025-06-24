using ModelContextProtocol.Server;
using System.ComponentModel;

namespace McpServer;

/// <summary>
/// Example prompts demonstrating different usage patterns for the MCP server.
/// </summary>
[McpServerPromptType]
public static class SamplePrompts
{
    // ----- Accept dynamic arguments -----

    /// <summary>
    /// Used in localization pipelines where the language varies per request.
    /// </summary>
    [McpServerPrompt, Description("Translate arbitrary input into the specified language.")]
    public static string Translate(string language, string input) =>
        $"Translate the following text to {language}: {input}";

    /// <summary>
    /// Generating targeted summaries for marketing or training materials.
    /// </summary>
    [McpServerPrompt, Description("Summarize content for a particular audience.")]
    public static string AudienceSummary(string audience, string content) =>
        $"""
Summarize the following for an audience of {audience}:
{content}
""";

    /// <summary>
    /// Producing region-specific reports.
    /// </summary>
    [McpServerPrompt, Description("Format a number for a given culture.")]
    public static string FormatNumber(string culture, decimal value) =>
        $"Format the value {value} using {culture} number formatting.";

    // ----- Include context from resources -----

    /// <summary>
    /// Knowledge base lookup from stored documents.
    /// </summary>
    [McpServerPrompt, Description("Answer a question using an external article as context.")]
    public static string ArticleQuestion(string article, string question) =>
        $"""
Based on the article below, answer the question.
Article:
{article}
Question: {question}
""";

    /// <summary>
    /// CRM systems generating custom communications.
    /// </summary>
    [McpServerPrompt, Description("Compose a personalized email using a customer record.")]
    public static string CustomerEmail(string record) =>
        $"""
Given the customer record:
{record}
Compose a short email response.
""";

    /// <summary>
    /// Developer tools that show example usage.
    /// </summary>
    [McpServerPrompt, Description("Generate an API request from documentation.")]
    public static string ApiDoc(string documentation) =>
        $"""
Use the following API documentation to craft an example request:
{documentation}
""";

    // ----- Chain multiple interactions -----

    /// <summary>
    /// Start a multi-turn research conversation.
    /// </summary>
    [McpServerPrompt, Description("Initial prompt asking the user for a topic.")]
    public static string ResearchStepOne() =>
        "Ask the user for a topic of interest.";

    /// <summary>
    /// Fetch information before continuing the chat.
    /// </summary>
    [McpServerPrompt, Description("Follow-up prompt that performs research.")]
    public static string ResearchStepTwo(string topic) =>
        $"Research {topic} and provide a brief summary.";

    /// <summary>
    /// Keeps the conversation going after delivering information.
    /// </summary>
    [McpServerPrompt, Description("Final prompt suggesting further questions.")]
    public static string ResearchStepThree(string topic) =>
        $"Suggest three follow-up questions about {topic}.";

    // ----- Guide specific workflows -----

    /// <summary>
    /// Standardizing bug triage notes.
    /// </summary>
    [McpServerPrompt, Description("Template for creating structured bug reports.")]
    public static string BugReport() =>
        "You are a bug triage assistant. Gather issue details and produce a concise report.";

    /// <summary>
    /// Automate planning discussions.
    /// </summary>
    [McpServerPrompt, Description("Template for crafting meeting agendas.")]
    public static string MeetingAgenda(int duration, string subject) =>
        $"Create an agenda for a {duration}-minute meeting about {subject}.";

    /// <summary>
    /// Ensure consistent review coverage.
    /// </summary>
    [McpServerPrompt, Description("Checklist to guide a code review session.")]
    public static string CodeReviewChecklist(string checklist) =>
        $"""
Guide the reviewer through this checklist:
{checklist}
""";
}
