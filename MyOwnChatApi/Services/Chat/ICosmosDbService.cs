using MyOwnChatApi.Domain.Models;

namespace MyOwnChatApi.Services.Chat
{
    public interface ICosmosDbService
    {
        Task<GptConversation?> GetConversationAsync(string userId, string conversationId);
        Task<List<Message>> GetLast3TurnsAsync(string userId, string conversationId);
        Task<string> CreateOrUpdateConversationAsync(GptConversation conversation);
        Task<List<ConversationSummary>> GetConversationListAsync(string userId);
    }
}
