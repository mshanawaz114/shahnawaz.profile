using ShahnawazProfile.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Controllers + JSON
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// CORS — permissive for local dev and for Azure SWA where client + linked API share origin.
// In production on Azure Static Web Apps with a linked API, calls are same-origin via /api/* proxy.
const string CorsPolicy = "PortfolioCors";
builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicy, policy =>
    {
        var origins = builder.Configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>() ?? Array.Empty<string>();

        if (origins.Length == 0)
        {
            policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
        }
        else
        {
            policy.WithOrigins(origins)
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        }
    });
});

// Groq chat service — typed HttpClient
builder.Services.AddHttpClient<IGroqChatService, GroqChatService>(client =>
{
    client.BaseAddress = new Uri("https://api.groq.com/");
    client.Timeout = TimeSpan.FromSeconds(30);
});

// Prompt builder + content provider
builder.Services.AddSingleton<IPromptBuilder, PromptBuilder>();
builder.Services.AddSingleton<IContentProvider, ContentProvider>();

var app = builder.Build();

app.UseCors(CorsPolicy);
app.MapControllers();

// Simple health endpoint
app.MapGet("/api/health", () => Results.Ok(new { status = "ok", runtime = ".NET 10" }));

app.Run();
