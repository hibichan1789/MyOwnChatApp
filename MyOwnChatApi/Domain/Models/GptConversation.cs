using Newtonsoft.Json;


namespace MyOwnChatApi.Domain.Models
{
    // CosmosDB用のモデルクラス
    public class GptConversation
    {
        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;

        // PartitionKey
        [JsonProperty("userId")]
        public string UserId { get; set; } = string.Empty;
        [JsonProperty("conversationId")]
        public string ConversationId { get; set; } = string.Empty;
        [JsonProperty("messages")]

        public IEnumerable<Message> Messages { get; set; } = [];

        [JsonProperty("summary")]
        public string Summary { get; set; } = string.Empty;
    }

    public class Message
    {
        [JsonProperty("role")]
        public string Role { get; set; } = string.Empty;
        [JsonProperty("content")]
        public string Content { get; set; } = string.Empty;
        [JsonProperty("timestamp")]
        public DateTimeOffset Timestamp { get; set; }
    }
}
