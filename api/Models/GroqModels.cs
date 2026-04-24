using System.Text.Json.Serialization;

namespace ShahnawazProfile.Api.Models;

// Request/response shapes for Groq's OpenAI-compatible REST API.

public class GroqCompletionRequest
{
    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;

    [JsonPropertyName("messages")]
    public List<ChatMessage> Messages { get; set; } = new();

    [JsonPropertyName("temperature")]
    public double Temperature { get; set; } = 0.4;

    [JsonPropertyName("max_tokens")]
    public int MaxTokens { get; set; } = 800;
}

public class GroqCompletionResponse
{
    [JsonPropertyName("choices")]
    public List<GroqChoice> Choices { get; set; } = new();
}

public class GroqChoice
{
    [JsonPropertyName("message")]
    public ChatMessage Message { get; set; } = new();

    [JsonPropertyName("finish_reason")]
    public string? FinishReason { get; set; }
}
