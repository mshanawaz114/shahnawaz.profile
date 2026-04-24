using System.Text;
using System.Text.Json;

namespace ShahnawazProfile.Api.Services;

public interface IPromptBuilder
{
    string BuildSystemPrompt();
}

public class PromptBuilder : IPromptBuilder
{
    private readonly IContentProvider _content;
    private string? _cached;

    public PromptBuilder(IContentProvider content)
    {
        _content = content;
    }

    public string BuildSystemPrompt()
    {
        if (_cached is not null) return _cached;

        var resume = _content.GetResume();
        var projects = _content.GetProjects();

        var sb = new StringBuilder();
        sb.AppendLine("You are the résumé assistant for Shahnawaz Mohammed — a Senior .NET Engineer and Solutions Architect.");
        sb.AppendLine("Answer questions from recruiters, hiring managers, and fellow engineers using ONLY the structured résumé and case-study data provided below.");
        sb.AppendLine();
        sb.AppendLine("Voice guidelines:");
        sb.AppendLine("- Formal, concise, third-person. Never refer to yourself as 'Shahnawaz' or speak as him.");
        sb.AppendLine("- Cite concrete technologies, years, and outcomes from the data.");
        sb.AppendLine("- If a question is outside the provided data, say so briefly and suggest contacting him via email.");
        sb.AppendLine("- Keep replies under 180 words unless the user explicitly asks for depth.");
        sb.AppendLine();
        sb.AppendLine("=== RESUME (JSON) ===");
        sb.AppendLine(JsonSerializer.Serialize(resume));
        sb.AppendLine();
        sb.AppendLine("=== PROJECTS (JSON) ===");
        sb.AppendLine(JsonSerializer.Serialize(projects));

        _cached = sb.ToString();
        return _cached;
    }
}
