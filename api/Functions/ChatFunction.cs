using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using ShahnawazProfile.Api.Models;
using ShahnawazProfile.Api.Services;

namespace ShahnawazProfile.Api.Functions;

/// <summary>
/// HTTP-triggered function exposing the résumé assistant chat endpoint.
/// Deployed as part of the Static Web Apps managed Functions API; routed at /api/chat.
/// Replaces the prior ASP.NET Core ChatController, which was incompatible with SWA's
/// managed API runtime (which only accepts Azure Functions, not full Web API projects).
/// </summary>
public class ChatFunction
{
    private static readonly JsonSerializerOptions DeserializeOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IGroqChatService _chat;
    private readonly IPromptBuilder _prompt;
    private readonly ILogger<ChatFunction> _logger;

    public ChatFunction(IGroqChatService chat, IPromptBuilder prompt, ILogger<ChatFunction> logger)
    {
        _chat = chat;
        _prompt = prompt;
        _logger = logger;
    }

    [Function("chat")]
    public async Task<IActionResult> Chat(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "chat")] HttpRequest req,
        CancellationToken cancellationToken)
    {
        ChatRequest? body;
        try
        {
            body = await JsonSerializer.DeserializeAsync<ChatRequest>(
                req.Body,
                DeserializeOptions,
                cancellationToken);
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Invalid JSON body");
            return new BadRequestObjectResult(new { error = "invalid JSON body" });
        }

        if (body is null || body.Messages is null || body.Messages.Count == 0)
        {
            return new BadRequestObjectResult(new { error = "messages[] required" });
        }

        // Clamp history to last 12 turns to bound prompt size.
        var recent = body.Messages
            .Where(m => !string.IsNullOrWhiteSpace(m.Content))
            .TakeLast(12)
            .ToList();

        var composed = new List<ChatMessage>
        {
            new() { Role = "system", Content = _prompt.BuildSystemPrompt() }
        };
        composed.AddRange(recent);

        try
        {
            var reply = await _chat.CompleteAsync(composed, cancellationToken);
            return new OkObjectResult(new ChatResponse { Reply = reply });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Chat configuration error");
            return new ObjectResult(new { error = "chat service is not configured on the server" })
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Upstream Groq error");
            return new ObjectResult(new { error = "chat upstream temporarily unavailable" })
            {
                StatusCode = StatusCodes.Status502BadGateway
            };
        }
    }

    [Function("health")]
    public IActionResult Health(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "health")] HttpRequest req)
    {
        return new OkObjectResult(new
        {
            status = "ok",
            runtime = ".NET 8 isolated (Azure Functions on Static Web Apps managed API)"
        });
    }
}
