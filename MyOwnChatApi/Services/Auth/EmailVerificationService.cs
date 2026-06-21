using Microsoft.EntityFrameworkCore;
using MyOwnChatApi.Context;

namespace MyOwnChatApi.Services.Auth
{
    public class EmailVerificationService:IEmailVerificationService
    {
        private readonly ILogger<EmailVerificationService> _logger;
        private readonly MyContext _db;

        public EmailVerificationService(ILogger<EmailVerificationService> logger, MyContext db)
        {
            _logger = logger;
            _db = db;
        }

        public async Task VerifyEmailAsync(string token)
        {
            var tokenHash = SecurityService.HashToken(token);

            var user = await _db.Users
                .FirstOrDefaultAsync(u => u.EmailVerificationTokenHash == tokenHash);

            if (user == null)
            {
                throw new InvalidOperationException("無効なトークンです");
            }

            if (user.EmailVerificationTokenExpiresAt == null ||
                user.EmailVerificationTokenExpiresAt < DateTimeOffset.UtcNow)
            {
                throw new InvalidOperationException("トークンの有効期限が切れています");
            }

            user.IsVerified = true;
            user.EmailVerificationTokenHash = null;
            user.EmailVerificationTokenExpiresAt = null;
            await _db.SaveChangesAsync();
            _logger.LogInformation("UserId={UserId},Email={Email}のユーザーの本登録が完了しました", user.Id, user.Email);
        }
    }
}
