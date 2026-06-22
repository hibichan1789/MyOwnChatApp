using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyOwnChatApi.Domain.DTOs.Chat;
using MyOwnChatApi.Services.Chat;
using System.Security.Claims;

namespace MyOwnChatApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly ILogger<ChatController> _logger;
        private readonly ICosmosDbService _cosmos;
        private readonly IChatService _chatService;

        public ChatController(
            ILogger<ChatController> logger,
            ICosmosDbService cosmos,
            IChatService chatService)
        {
            _logger = logger;
            _cosmos = cosmos;
            _chatService = chatService;
        }

        // POST: /api/chat/send 同期版
        [HttpPost("send")]
        public async Task<ActionResult> SendMessage(ChatRequestDto chatRequest)
        {
            var userClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userClaim == null)
            {
                return Unauthorized(new { message = "ユーザー情報が取得できません" });
            }
            string userId = userClaim.Value;

            var result = await _chatService.SendMessageAsync(userId, chatRequest);

            return Ok(result);
        }

        // POST: /api/chat/stream Stream版
        [HttpPost("stream")]
        public async Task StreamMessage(ChatRequestDto chatRequest)
        {
            var userClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userClaim == null)
            {
                Response.StatusCode = StatusCodes.Status401Unauthorized;
                await Response.WriteAsync("data: Unauthorized\n\n");
                return;
            }
            string userId = userClaim.Value;

            if (string.IsNullOrWhiteSpace(chatRequest.ConversationId))
            {
                chatRequest.ConversationId = Guid.NewGuid().ToString();
            }

            // SSEレスポンスヘッダの作成
            Response.Headers.Add("Content-Type", "text/event-stream");
            Response.Headers.Add("Cache-Control", "no-cache");
            Response.Headers.Add("Connection", "keep-alive");

            // 最初に conversationIdを送る(フロントエンドが必要なため)
            await Response.WriteAsync($"data: {{\"conversationId\":\"{chatRequest.ConversationId}\"}}\n\n");
            await Response.Body.FlushAsync();

            // ChatServiceのストリームを受け取ってそのまま流す
            await foreach(var delta in _chatService.SendMessageStreamAsync(
                    userId,
                    chatRequest,
                    HttpContext.RequestAborted
                ))
            {
                // SSE形式で送信
                await Response.WriteAsync($"data: {delta}\n\n");
                await Response.Body.FlushAsync();
            }

            await Response.WriteAsync("data: [DONE]\n\n");
            await Response.Body.FlushAsync();
        }

        // GET /api/history 会話一覧
        [HttpGet("history")]
        public async Task<ActionResult> GetHistryList()
        {
            var userClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userClaim == null)
            {
                return Unauthorized(new { message = "ユーザー情報が取得できません" });
            }
            string userId = userClaim.Value;

            var list = await _cosmos.GetConversationListAsync(userId);

            return Ok(list);
        }

        // GET: /api/history/{conversationId}
        [HttpGet("history/{conversationId}")]
        public async Task<ActionResult> GetHistoryDetail(string conversationId)
        {
            var userClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userClaim == null)
            {
                return Unauthorized(new { message = "ユーザー情報が取得できません" });
            }
            string userId = userClaim.Value;

            var conversation = await _cosmos.GetConversationAsync(userId, conversationId);

            if(conversation == null)
            {
                return NotFound();
            }

            return Ok(conversation);
        }
    }
}
