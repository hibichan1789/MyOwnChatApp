namespace MyOwnChatApi.Domain.DTOs.Chat
{
    public class AiReplyResultDto
    {
        public string Reply { get; set; } = string.Empty;
        public int PromptTokens { get; set; }
        public int CompletionTokens { get; set; }
    }
}
