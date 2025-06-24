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
    /// Translate arbitrary input into the specified language.
    /// Real world: used in localization pipelines where the language varies per request.
    /// </summary>
    [McpServerPrompt, Description("Translate arbitrary input into the specified language. Real world: used in localization pipelines where the language varies per request.")]
    public static string Translate(string language, string input) =>
        $"Translate the following text to {language}: {input}";

    /// <summary>
    /// Summarize content for a particular audience.
    /// Real world: generating targeted summaries for marketing or training materials.
    /// </summary>
    [McpServerPrompt, Description("Summarize content for a particular audience. Real world: generating targeted summaries for marketing or training materials.")]
    public static string AudienceSummary(string audience, string content) =>
        $"Summarize the following for an audience of {audience}:{Environment.NewLine}{content}";

    /// <summary>
    /// Format a number for a given culture.
    /// Real world: producing region-specific reports.
    /// </summary>
    [McpServerPrompt, Description("Format a number for a given culture. Real world: producing region-specific reports.")]
    public static string FormatNumber(string culture, decimal value) =>
        $"Format the value {value} using {culture} number formatting.";

    // ----- Include context from resources -----

    /// <summary>
    /// Answer a question using an external article as context.
    /// Real world: knowledge base lookup from stored documents.
    /// </summary>
    [McpServerPrompt, Description("Answer a question using an external article as context. Real world: knowledge base lookup from stored documents.")]
    public static string ArticleQuestion(string article, string question) =>
        $"Based on the article below, answer the question.{Environment.NewLine}Article:{Environment.NewLine}{article}{Environment.NewLine}Question: {question}";

    /// <summary>
    /// Compose a personalized email using a customer record.
    /// Real world: CRM systems generating custom communications.
    /// </summary>
    [McpServerPrompt, Description("Compose a personalized email using a customer record. Real world: CRM systems generating custom communications.")]
    public static string CustomerEmail(string record) =>
        $"Given the customer record:{Environment.NewLine}{record}{Environment.NewLine}Compose a short email response.";

    /// <summary>
    /// Generate an API request from documentation.
    /// Real world: developer tools that show example usage.
    /// </summary>
    [McpServerPrompt, Description("Generate an API request from documentation. Real world: developer tools that show example usage.")]
    public static string ApiDoc(string documentation) =>
        $"Use the following API documentation to craft an example request:{Environment.NewLine}{documentation}";

    // ----- Chain multiple interactions -----

    /// <summary>
    /// Initial prompt asking the user for a topic.
    /// Real world: start a multi-turn research conversation.
    /// </summary>
    [McpServerPrompt, Description("Initial prompt asking the user for a topic. Real world: start a multi-turn research conversation.")]
    public static string ResearchStepOne() =>
        "Ask the user for a topic of interest.";

    /// <summary>
    /// Follow-up prompt that performs research.
    /// Real world: fetch information before continuing the chat.
    /// </summary>
    [McpServerPrompt, Description("Follow-up prompt that performs research. Real world: fetch information before continuing the chat.")]
    public static string ResearchStepTwo(string topic) =>
        $"Research {topic} and provide a brief summary.";

    /// <summary>
    /// Final prompt suggesting further questions.
    /// Real world: keeps the conversation going after delivering information.
    /// </summary>
    [McpServerPrompt, Description("Final prompt suggesting further questions. Real world: keeps the conversation going after delivering information.")]
    public static string ResearchStepThree(string topic) =>
        $"Suggest three follow-up questions about {topic}.";

    // ----- Guide specific workflows -----

    /// <summary>
    /// Template for creating structured bug reports.
    /// Real world: standardizing bug triage notes.
    /// </summary>
    [McpServerPrompt, Description("Template for creating structured bug reports. Real world: standardizing bug triage notes.")]
    public static string BugReport() =>
        "You are a bug triage assistant. Gather issue details and produce a concise report.";

    /// <summary>
    /// Template for crafting meeting agendas.
    /// Real world: automate planning discussions.
    /// </summary>
    [McpServerPrompt, Description("Template for crafting meeting agendas. Real world: automate planning discussions.")]
    public static string MeetingAgenda(int duration, string subject) =>
        $"Create an agenda for a {duration}-minute meeting about {subject}.";

    /// <summary>
    /// Checklist to guide a code review session.
    /// Real world: ensure consistent review coverage.
    /// </summary>
    [McpServerPrompt, Description("Checklist to guide a code review session. Real world: ensure consistent review coverage.")]
    public static string CodeReviewChecklist(string checklist) =>
        $"Guide the reviewer through this checklist:{Environment.NewLine}{checklist}";
}
