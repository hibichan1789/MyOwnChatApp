using MyOwnChatApi.Domain.DTOs.Chat;
using System.Runtime.CompilerServices;

namespace MyOwnChatApi.Services.Chat
{
    public interface IChatService
    {
        Task<ChatResponseDto> SendMessageAsync(string userId, ChatRequestDto  chatRequest);
        IAsyncEnumerable<string> SendMessageStreamAsync(string userId, ChatRequestDto chatRequest, [EnumeratorCancellation] CancellationToken cancellationToken = default);
    }
}
