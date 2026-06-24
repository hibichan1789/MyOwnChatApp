using MyOwnChatApi.Domain.Models;

namespace MyOwnChatApi.Domain.DTOs.Chat
{ 
        // CosmosDB用のモデルクラス
        public class ConversationDto
        {
            public string ConversationId { get; set; } = string.Empty;
            
            public IEnumerable<Message> Messages { get; set; } = [];
        }
}
