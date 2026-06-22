
using MyOwnChatApi.Domain.Models;
using Azure.AI.OpenAI;
using System.ClientModel;
using OpenAI.Chat;
using System.Runtime.CompilerServices;
using System.Text;



namespace MyOwnChatApi.Services.Chat
{
    public class AiFoundryService:IAiFoundryService
    {
        private readonly ILogger<AiFoundryService> _logger;
        private readonly ChatClient _cahtClient;
        public AiFoundryService(ILogger<AiFoundryService> logger, IConfiguration config)
        {
            _logger = logger;

            var endpoint = config["AzureAI:Endpoint"];
            var apiKey = config["AzureAI:ApiKey"];
            var deployModel = config["AzureAI:DeploymentName"];
            var client = new AzureOpenAIClient(new Uri(endpoint), new ApiKeyCredential(apiKey));

            _cahtClient = client.GetChatClient(deployModel);
        }

        // 同期版
        public async Task<string> GenerateReplyAsync(string summary, List<Message> contextMessages, string userMessage)
        {
            var chatMessages = new List<ChatMessage>();
            if (!string.IsNullOrWhiteSpace(summary))
            {
                chatMessages.Add(new SystemChatMessage($"これまでの会話の要約:\n{summary}"));
            }

            // 直近の会話履歴をコンテキストとして追加
            foreach(var msg in contextMessages)
            {
                if (msg.Role == "user")
                {
                    chatMessages.Add(new UserChatMessage(msg.Content));
                }
                else
                {
                    chatMessages.Add(new AssistantChatMessage(msg.Content));
                }
            }

            // 新しいユーザー入力を追加
            chatMessages.Add(new UserChatMessage(userMessage));

            _logger.LogInformation("AI Foundryにリクエスト送信中...");

            var response = await _cahtClient.CompleteChatAsync(chatMessages);

            var reply = response.Value.Content[0].Text;

            _logger.LogInformation("AI Foundryからの返答: {Reply}", reply);

            return reply;
        }

        // Stream版
        public async IAsyncEnumerable<string> GenerateReplyStreamAsync(
            string summary,
            List<Message> contextMessages,
            string userMessage,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var chatMessages = new List<ChatMessage>();
            if (!string.IsNullOrWhiteSpace(summary))
            {
                chatMessages.Add(new SystemChatMessage($"これまでの会話の要約:\n{summary}"));
            }

            foreach(var msg in contextMessages)
            {
                if(msg.Role == "user")
                {
                    chatMessages.Add(new UserChatMessage(msg.Content));
                }
                else
                {
                    chatMessages.Add(new AssistantChatMessage(msg.Content));
                }
            }

            chatMessages.Add(new UserChatMessage(userMessage));

            _logger.LogInformation("AI Foundryにリクエスト送信中...");
            var streamingResult = _cahtClient.CompleteChatStreamingAsync(chatMessages, cancellationToken: cancellationToken);
            var sb = new StringBuilder();

            await foreach(var update in streamingResult.WithCancellation(cancellationToken))
            {
                if(update.ContentUpdate == null)
                {
                    continue;
                }
                // ContentUpdateにdeltaが入ってくる
                foreach(var contentPart in update.ContentUpdate)
                {
                    var delta = contentPart.Text;
                    if (string.IsNullOrWhiteSpace(delta))
                    {
                        continue;
                    }

                    // 全文用に蓄積
                    sb.Append(delta);

                    // 呼び出し元にチャンクを返す
                    yield return delta;
                }
            }

            var fullReply = sb.ToString();
            _logger.LogInformation("[AiFoundryService]AI Foundryからの返答(stream full): {Reply}", fullReply);
        }

        public async Task<string> GenerateSummaryAsync(string previousSummary, List<Message> newTurns)
        {
            var messages = new List<ChatMessage>();

            if (!string.IsNullOrWhiteSpace(previousSummary))
            {
                messages.Add(new SystemChatMessage($"これまでの会話の要約:\n {previousSummary}"));
            }

            // 新しくAIで生成された会話
            foreach(var msg in newTurns)
            {
                if(msg.Role == "user")
                {
                    messages.Add(new UserChatMessage(msg.Content) );
                }
                else
                {
                    messages.Add(new AssistantChatMessage(msg.Content));
                }
            }

            // 要約指示用のプロンプト
            messages.Add(
                new SystemChatMessage("上記の内容を踏まえて、会話の重要な情報の要約を短く作成してください。要約は最大300文字に圧縮してください。\n" +
                "不要な挨拶や雑談は含めないでください。")
                );

            var response = await _cahtClient.CompleteChatAsync(messages);
            return response.Value.Content[0].Text;
        }
    }
}
