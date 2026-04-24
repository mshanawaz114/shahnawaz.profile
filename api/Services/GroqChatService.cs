using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ShahnawazProfile.Api.Models;

namespace ShahnawazProfile.Api.Services;

public class GroqChatService : IGroqChatService
{
    private readonly HttpClient _http;
    private readonly IConfiguration _config;
    private readonly ILogger<GroqChatService> _logger;

    public GroqChatService(HttpClient http, IConfiguration config, ILogger<GroqChatService> logger)
    {
        _http = http;
        _config = config;
        _logger = logger;
    }

    public async Task<string> CompleteAsync(IEnumerable<ChatMessage> messages, CancellationToken cancellationToken)
    {
        var apiKey = _config["GROQ_API_KEY"]
                     ?? Environment.GetEnvironmentVariable("GROQ_API_KEY");

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException(
                "GROQ_API_KEY is not configured. Set it via environment variable, user-secrets, or Azure App Service configuration.");
        }

        var payload = new GroqCompletionRequest
        {
            // Default to llama-3.1-8b-instant: same Llama family as 3.3-70b but with a much
            // higher TPM ceiling on Groq's free tier. The résumé Q&A use case is small-context
            // factual lookup — the 8B model handles it cleanly without burning rate-limit budget.
            // Override via the Groq:Model setting (appsettings/local.settings/SWA app setting).
            Model = _config["Groq:Model"] ?? "llama-3.1-8b-instant",
            Temperature = double.TryParse(_config["Groq:Temperature"], out var t) ? t : 0.4,
            MaxTokens = int.TryParse(_config["Groq:MaxTokens"], out var m) ? m : 800,
            Messages = messages.ToList()
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, "openai/v1/chat/completions")
        {
            Content = JsonContent.Create(payload)
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

        using var response = await _http.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogWarning("Groq API call failed. Status: {Status}, Body: {Body}", response.StatusCode, body);
            throw new HttpRequestException($"Groq API returned {(int)response.StatusCode}: {response.ReasonPhrase}");
        }

        var completion = await response.Content.ReadFromJsonAsync<GroqCompletionResponse>(cancellationToken: cancellationToken);
        var reply = completion?.Choices.FirstOrDefault()?.Message.Content;

        return string.IsNullOrWhiteSpace(reply)
            ? "I wasn't able to produce a response just now. Please try again."
            : reply;
    }
}
