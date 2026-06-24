using MyOwnChatApi.Domain.DTOs.Chat;
using MyOwnChatApi.Domain.Models;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

namespace MyOwnChatApi.Services.Chat
{
    public class ChatService:IChatService
    {
        private readonly ILogger<ChatService> _logger;
        private readonly ICosmosDbService _cosmos;
        private readonly IAiFoundryService _ai;
        public ChatService(
            ILogger<ChatService> logger,
            ICosmosDbService cosmos,
            IAiFoundryService ai)
        {
            _logger = logger;
            _cosmos = cosmos;
            _ai = ai;
        }

        public async Task<ChatResponseDto> SendMessageAsync(string userId, ChatRequestDto chatRequest)
        {
            string conversationId = chatRequest.ConversationId ?? Guid.NewGuid().ToString();

            // CosmosDBに保存するためのモデルを作成
            var conversation = await _cosmos.GetConversationAsync(userId, conversationId)
            ?? new GptConversation
            {
                Id = Guid.NewGuid().ToString(),
                ConversationId = conversationId,
                UserId = userId,
                Messages = new List<Message>(),
                Summary = ""
            };
            

            // 直近3往復分の会話を取得
            var contextMessages = await _cosmos.GetLast3TurnsAsync(userId, conversationId);


            

            
            // AIに投げる(時間も計測する)
            Stopwatch stopwatch = Stopwatch.StartNew();
            var aiReply = await _ai.GenerateReplyAsync(conversation.Summary, contextMessages, chatRequest.Content);
            stopwatch.Stop();


            var updateMessages = conversation.Messages.ToList();
            // ユーザーの新規メッセージ
            var now = DateTime.UtcNow;
            Message newUserMessage = new Message
            {
                Role = "user",
                Content = chatRequest.Content,
                ConsumedTokens = aiReply.PromptTokens,
                Timestamp = now
            };
            updateMessages.Add(newUserMessage);

            Message newAiMessage = new Message
            {
                Role = "assistant",
                Content = aiReply.Reply,
                ConsumedTokens = aiReply.CompletionTokens,
                Timestamp = now + stopwatch.Elapsed
            };
            updateMessages.Add(newAiMessage);

            conversation.Messages = updateMessages;

            var newTurns = new List<Message>() { newUserMessage, newAiMessage };
            
            try
            {
                var newSummary = await _ai.GenerateSummaryAsync(conversation.Summary, newTurns);
                conversation.Summary = newSummary;
                _logger.LogInformation("newSummary: {newSummary}", newSummary);
            }
            catch (Exception ex)
            {
                _logger.LogError("Summary generation failed: {Message}", ex.Message);
                // summary は前回のまま維持
            }

            // CosmosDBに保存
            await _cosmos.CreateOrUpdateConversationAsync(conversation);
            

            return new ChatResponseDto
            {
                ConversationId = conversationId,
                Reply = aiReply.Reply
            };
        }

        // Stream版
        public async IAsyncEnumerable<string> SendMessageStreamAsync(string userId, ChatRequestDto chatRequest, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            string conversationId = chatRequest.ConversationId!;

            // 既存会話 or 新規会話取得
            var conversation = await _cosmos.GetConversationAsync(userId, conversationId)
                ?? new GptConversation
                {
                    Id = Guid.NewGuid().ToString(),
                    ConversationId = conversationId,
                    UserId = userId,
                    Messages = new List<Message>(),
                    Summary = ""
                };

            // 直近3往復分の会話を取得
            var contextMessages = await _cosmos.GetLast3TurnsAsync(userId, conversationId);


            var usageInfo = new UsageInfo();
            // AI返信をStreamで受け取る
            var sb = new StringBuilder();
            Stopwatch stopwatch = Stopwatch.StartNew();
            await foreach(var delta in _ai.GenerateReplyStreamAsync(
                conversation.Summary,
                contextMessages,
                chatRequest.Content,
                usageInfo,
                cancellationToken
                ).WithCancellation(cancellationToken))
            {
                // 全文ように蓄積
                sb.Append(delta);

                // 呼び出し元にチャンクを返す
                yield return delta;
            }
            stopwatch.Stop();


            var updateMessages = conversation.Messages.ToList();
            // ユーザーの新規メッセージ
            var now = DateTime.UtcNow;
            var newUserMessage = new Message
            {
                Role = "user",
                Content = chatRequest.Content,
                ConsumedTokens = usageInfo.PromptTokens,
                Timestamp = now
            };
            updateMessages.Add(newUserMessage);
            var fullReply = sb.ToString();
            var newAiMessage = new Message
            {
                Role = "assistant",
                Content = fullReply,
                ConsumedTokens = usageInfo.CompletionTokens,
                Timestamp = now + stopwatch.Elapsed
            };
            updateMessages.Add(newAiMessage);

            conversation.Messages = updateMessages;

            // 要約更新
            var newTurns = new List<Message> { newUserMessage, newAiMessage };

            try
            {
                var newSummary = await _ai.GenerateSummaryAsync(conversation.Summary, newTurns);
                conversation.Summary = newSummary;
                _logger.LogInformation("newSummary: {newSummary}", newSummary);
            }
            catch (Exception ex)
            {
                _logger.LogError("Summary generation failed: {Message}", ex.Message);
                // summary は前回のまま維持
            }

            // CosmosDBに保存
            await _cosmos.CreateOrUpdateConversationAsync(conversation);
        }
    }
}
