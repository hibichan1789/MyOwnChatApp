namespace MyOwnChatApi.Domain.DTOs.Chat
{
    public class ChatRequestDto
    {
        public string? ConversationId { get; set; }
        public string Content { get; set; } = string.Empty;
    }
}
