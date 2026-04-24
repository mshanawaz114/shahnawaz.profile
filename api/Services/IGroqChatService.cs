using ShahnawazProfile.Api.Models;

namespace ShahnawazProfile.Api.Services;

public interface IGroqChatService
{
    Task<string> CompleteAsync(IEnumerable<ChatMessage> messages, CancellationToken cancellationToken);
}
