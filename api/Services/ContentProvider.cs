using System.Text.Json;

namespace ShahnawazProfile.Api.Services;

public interface IContentProvider
{
    JsonDocument GetResume();
    JsonDocument GetProjects();
    string GetResumeRaw();
    string GetProjectsRaw();
}

/// <summary>
/// Loads the structured résumé and project JSON files used to seed the LLM system prompt.
/// In Azure Functions isolated worker the published artifacts (Data/*.json) sit next to
/// the assembly, so paths resolve via AppContext.BaseDirectory rather than IWebHostEnvironment.
/// </summary>
public class ContentProvider : IContentProvider
{
    private readonly Lazy<string> _resumeRaw;
    private readonly Lazy<string> _projectsRaw;
    private readonly Lazy<JsonDocument> _resumeDoc;
    private readonly Lazy<JsonDocument> _projectsDoc;

    public ContentProvider()
    {
        var dataDir = Path.Combine(AppContext.BaseDirectory, "Data");

        _resumeRaw = new Lazy<string>(() => File.ReadAllText(Path.Combine(dataDir, "resume.json")));
        _projectsRaw = new Lazy<string>(() => File.ReadAllText(Path.Combine(dataDir, "projects.json")));
        _resumeDoc = new Lazy<JsonDocument>(() => JsonDocument.Parse(_resumeRaw.Value));
        _projectsDoc = new Lazy<JsonDocument>(() => JsonDocument.Parse(_projectsRaw.Value));
    }

    public JsonDocument GetResume() => _resumeDoc.Value;
    public JsonDocument GetProjects() => _projectsDoc.Value;
    public string GetResumeRaw() => _resumeRaw.Value;
    public string GetProjectsRaw() => _projectsRaw.Value;
}
