using System.Text.Json.Serialization;

namespace MyOwnChatApi.Domain.Models
{
    public class ConversationSummary
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;
        [JsonPropertyName("conversationId")]
        public string ConversationId { get; set; } = string.Empty;
        [JsonPropertyName("firstMessage")]
        public Message FirstMessage { get; set; } = new();
    }
}
