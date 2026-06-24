using MyOwnChatApi.Domain.DTOs.Chat;
using MyOwnChatApi.Domain.Models;
using System.Runtime.CompilerServices;

namespace MyOwnChatApi.Services.Chat
{
    public interface IAiFoundryService
    {
        Task<AiReplyResultDto> GenerateReplyAsync(string summary, List<Message> contextMessages, string userMessage);
        IAsyncEnumerable<string> GenerateReplyStreamAsync(string summary, List<Message> contextMessages, string userMessage, UsageInfo usageInfo, [EnumeratorCancellation] CancellationToken cancellationToken = default);
        Task<string> GenerateSummaryAsync(string previousSummary, List<Message> newTurns);
    }
}
