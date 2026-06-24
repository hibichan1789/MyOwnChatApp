using MyOwnChatApi.Domain.Models;

namespace MyOwnChatApi.Domain.DTOs.Chat
{ 
        public class ConversationDto
        {
            public string ConversationId { get; set; } = string.Empty;
            
            public IEnumerable<Message> Messages { get; set; } = [];
        }
}
