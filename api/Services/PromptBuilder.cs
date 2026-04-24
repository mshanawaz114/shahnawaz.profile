using System.Text;
using System.Text.Json;

namespace ShahnawazProfile.Api.Services;

public interface IPromptBuilder
{
    string BuildSystemPrompt();
}

/// <summary>
/// Builds the system prompt from resume.json + projects.json.
/// IMPORTANT: We render a dense plaintext digest (~3–4 KB) instead of dumping raw JSON
/// (~30 KB). The raw JSON blew past Groq's free-tier 6,000 TPM ceiling on the second
/// turn of any conversation, surfacing as 502 "upstream temporarily unavailable" errors
/// in the chat widget. A plaintext digest gives the model the same factual coverage at
/// roughly one-eighth the token cost.
/// </summary>
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

        var sb = new StringBuilder();
        sb.AppendLine("You are the résumé assistant for Shahnawaz Mohammed — a Senior .NET Engineer and Solutions Architect.");
        sb.AppendLine("Answer questions from recruiters, hiring managers, and fellow engineers using ONLY the digest below.");
        sb.AppendLine();
        sb.AppendLine("Voice guidelines:");
        sb.AppendLine("- Formal, concise, third-person. Never speak as Shahnawaz; refer to him by name or 'he'.");
        sb.AppendLine("- Cite concrete technologies, years, and outcomes from the digest.");
        sb.AppendLine("- If a question is outside the digest, say so briefly and suggest emailing mshanawaz.net@gmail.com.");
        sb.AppendLine("- Keep replies under 180 words unless the user explicitly asks for depth.");
        sb.AppendLine();

        AppendResumeDigest(sb, _content.GetResume());
        sb.AppendLine();
        AppendProjectsDigest(sb, _content.GetProjects());

        _cached = sb.ToString();
        return _cached;
    }

    // ─── Resume digest ────────────────────────────────────────────────────────

    private static void AppendResumeDigest(StringBuilder sb, JsonDocument doc)
    {
        var root = doc.RootElement;

        sb.AppendLine("=== PROFILE ===");
        sb.AppendLine($"Name: {Get(root, "name")}");
        sb.AppendLine($"Title: {Get(root, "title")}");
        sb.AppendLine($"Location: {Get(root, "location")}");
        sb.AppendLine($"Email: {Get(root, "email")}");
        sb.AppendLine($"Phone: {Get(root, "phone")}");
        if (root.TryGetProperty("social", out var social))
        {
            var parts = new List<string>();
            foreach (var p in social.EnumerateObject())
            {
                parts.Add($"{p.Name}: {p.Value.GetString()}");
            }
            if (parts.Count > 0) sb.AppendLine($"Links: {string.Join(" | ", parts)}");
        }
        sb.AppendLine();

        if (root.TryGetProperty("summary", out var summary) && summary.ValueKind == JsonValueKind.Array)
        {
            sb.AppendLine("=== SUMMARY ===");
            foreach (var line in summary.EnumerateArray())
            {
                sb.AppendLine($"- {line.GetString()}");
            }
            sb.AppendLine();
        }

        if (root.TryGetProperty("skills", out var skills) && skills.ValueKind == JsonValueKind.Object)
        {
            sb.AppendLine("=== SKILLS (by category) ===");
            foreach (var category in skills.EnumerateObject())
            {
                var items = new List<string>();
                foreach (var s in category.Value.EnumerateArray())
                {
                    items.Add(s.GetString() ?? "");
                }
                sb.AppendLine($"{category.Name}: {string.Join(", ", items)}");
            }
            sb.AppendLine();
        }

        if (root.TryGetProperty("experience", out var experience) && experience.ValueKind == JsonValueKind.Array)
        {
            sb.AppendLine("=== EXPERIENCE (most recent first) ===");
            foreach (var job in experience.EnumerateArray())
            {
                var company = Get(job, "company");
                var role = Get(job, "role");
                var loc = Get(job, "location");
                var start = Get(job, "start");
                var end = Get(job, "end");
                sb.AppendLine($"• {company} — {role} ({loc}) | {start} → {end}");

                if (job.TryGetProperty("highlights", out var hl) && hl.ValueKind == JsonValueKind.Array)
                {
                    foreach (var h in hl.EnumerateArray())
                    {
                        sb.AppendLine($"   - {h.GetString()}");
                    }
                }

                if (job.TryGetProperty("stack", out var stack) && stack.ValueKind == JsonValueKind.Array)
                {
                    var techs = new List<string>();
                    foreach (var t in stack.EnumerateArray()) techs.Add(t.GetString() ?? "");
                    sb.AppendLine($"   Stack: {string.Join(", ", techs)}");
                }
            }
            sb.AppendLine();
        }

        if (root.TryGetProperty("education", out var education) && education.ValueKind == JsonValueKind.Array)
        {
            sb.AppendLine("=== EDUCATION ===");
            foreach (var ed in education.EnumerateArray())
            {
                var school = Get(ed, "school");
                var loc = Get(ed, "location");
                var degree = Get(ed, "degree");
                var start = Get(ed, "start");
                var end = Get(ed, "end");
                var notes = Get(ed, "notes");
                var dates = string.IsNullOrEmpty(start) ? end : $"{start} → {end}";
                sb.AppendLine($"• {degree} — {school} ({loc}) | {dates}");
                if (!string.IsNullOrEmpty(notes)) sb.AppendLine($"   {notes}");
            }
            sb.AppendLine();
        }

        if (root.TryGetProperty("certifications", out var certs) && certs.ValueKind == JsonValueKind.Array)
        {
            sb.AppendLine("=== CERTIFICATIONS ===");
            foreach (var c in certs.EnumerateArray())
            {
                var name = Get(c, "name");
                var id = Get(c, "id");
                var issuer = Get(c, "issuer");
                sb.AppendLine($"- {name} ({id}, {issuer})");
            }
            sb.AppendLine();
        }

        if (root.TryGetProperty("interests", out var interests) && interests.ValueKind == JsonValueKind.Array)
        {
            sb.AppendLine("=== INTERESTS ===");
            foreach (var i in interests.EnumerateArray())
            {
                sb.AppendLine($"- {i.GetString()}");
            }
            sb.AppendLine();
        }
    }

    // ─── Projects digest ──────────────────────────────────────────────────────

    private static void AppendProjectsDigest(StringBuilder sb, JsonDocument doc)
    {
        var root = doc.RootElement;
        if (root.ValueKind != JsonValueKind.Array) return;

        sb.AppendLine("=== PROJECT CASE STUDIES ===");
        foreach (var p in root.EnumerateArray())
        {
            var name = Get(p, "name");
            var client = Get(p, "client");
            var headline = Get(p, "headline");
            sb.AppendLine($"• {name} — {client}");
            if (!string.IsNullOrEmpty(headline)) sb.AppendLine($"   Headline: {headline}");

            if (p.TryGetProperty("impact", out var impact) && impact.ValueKind == JsonValueKind.Array)
            {
                foreach (var i in impact.EnumerateArray())
                {
                    sb.AppendLine($"   - {i.GetString()}");
                }
            }

            if (p.TryGetProperty("stack", out var stack) && stack.ValueKind == JsonValueKind.Array)
            {
                var techs = new List<string>();
                foreach (var t in stack.EnumerateArray()) techs.Add(t.GetString() ?? "");
                sb.AppendLine($"   Stack: {string.Join(", ", techs)}");
            }
        }
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────

    private static string Get(JsonElement el, string prop) =>
        el.TryGetProperty(prop, out var v) && v.ValueKind == JsonValueKind.String
            ? v.GetString() ?? ""
            : "";
}
