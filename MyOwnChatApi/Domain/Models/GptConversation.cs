using System.Text.Json.Serialization;

namespace MyOwnChatApi.Domain.Models
{
    // CosmosDB用のモデルクラス
    public class GptConversation
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("userId")]
        public int UserId { get; set; }
        [JsonPropertyName("conversationId")]
        public string ConvesationId { get; set; } = string.Empty;

        public IEnumerable<Message> Messages { get; set; } = [];
    }

    public class Message
    {
        [JsonPropertyName("role")]
        public string Role { get; set; } = string.Empty;
        [JsonPropertyName("content")]
        public string Content { get; set; } = string.Empty;
        [JsonPropertyName("timestamp")]
        public DateTimeOffset Timestamp { get; set; }
    }
}
