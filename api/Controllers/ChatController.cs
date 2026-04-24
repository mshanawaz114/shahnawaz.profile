using Microsoft.AspNetCore.Mvc;
using ShahnawazProfile.Api.Models;
using ShahnawazProfile.Api.Services;

namespace ShahnawazProfile.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly IGroqChatService _chat;
    private readonly IPromptBuilder _prompt;
    private readonly ILogger<ChatController> _logger;

    public ChatController(IGroqChatService chat, IPromptBuilder prompt, ILogger<ChatController> logger)
    {
        _chat = chat;
        _prompt = prompt;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] ChatRequest request, CancellationToken cancellationToken)
    {
        if (request is null || request.Messages is null || request.Messages.Count == 0)
        {
            return BadRequest(new { error = "messages[] required" });
        }

        // Clamp history to last 12 turns to bound prompt size
        var recent = request.Messages
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
            return Ok(new ChatResponse { Reply = reply });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Chat configuration error");
            return StatusCode(500, new { error = "chat service is not configured on the server" });
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Upstream Groq error");
            return StatusCode(502, new { error = "chat upstream temporarily unavailable" });
        }
    }
}
