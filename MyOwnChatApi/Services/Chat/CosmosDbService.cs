using Microsoft.Azure.Cosmos;
using MyOwnChatApi.Domain.Models;



namespace MyOwnChatApi.Services.Chat
{
    public class CosmosDbService : ICosmosDbService
    {
        private readonly ILogger<CosmosDbService> _logger;
        private readonly Container _container;

        public CosmosDbService(ILogger<CosmosDbService> logger,IConfiguration config)
        {
            _logger = logger;

            var client = new CosmosClient(
                    config["CosmosDb:Endpoint"],
                    config["CosmosDb:Key"]
                );
            var database = client.GetDatabase(config["CosmosDb:Database"]);
            _container = database.GetContainer(config["CosmosDb:Container"]);
        }


        public async Task<GptConversation?> GetConversationAsync(string userId, string conversationId)
        {
            var query = new QueryDefinition(
                    "SELECT * FROM c WHERE c.userId = @uid AND c.conversationId = @cid"
                )
                .WithParameter("@uid", userId)
                .WithParameter("@cid", conversationId);

            var iterator = _container.GetItemQueryIterator<GptConversation>(query);

            if (iterator.HasMoreResults)
            {
                var result = await iterator.ReadNextAsync();
                return result.FirstOrDefault();
            }


            _logger.LogInformation("会話は見つかりませんでした");
            return null;
        } 

        public async Task<List<Message>> GetLast3TurnsAsync(string userId, string conversationId)
        {
            var conversations = await GetConversationAsync(userId, conversationId);
            
            if(conversations == null)
            {
                return new List<Message>();
            }


            return conversations.Messages
                .OrderByDescending(m => m.Timestamp)
                .Take(6)
                .OrderBy(m => m.Timestamp)
                .ToList();
        }

        public async Task<string> CreateOrUpdateConversationAsync(GptConversation conversation)
        {
            var response = await _container.UpsertItemAsync(
                    conversation,
                    new PartitionKey(conversation.UserId)
                    );

            return response.Resource.Id;
        }

        public async Task<List<ConversationSummary>> GetConversationListAsync(string userId)
        {
            _logger.LogInformation("UserId={UserId}の会話サマリを取得します", userId);
            var query = new QueryDefinition(
                    "SELECT c.id, c.conversationId, c.messages[0] AS firstMessage " +
                    "FROM c WHERE c.userId = @uid"
                )
                .WithParameter("@uid", userId);

            var iterator = _container.GetItemQueryIterator<ConversationSummary>(query);

            var results = new List<ConversationSummary>();

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                results.AddRange(response);
            }

            return results
                .OrderByDescending(c => c.FirstMessage.Timestamp)
                .ToList();
        }
    }
}
