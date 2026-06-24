using MyOwnChatApi.Domain.Models;

namespace MyOwnChatApi.Domain.DTOs.Chat
{
    public class ConversationSummaryDto
    {
        public string ConversationId { get; set; } = string.Empty;
        
        public Message FirstMessage { get; set; } = new();
    }
}
