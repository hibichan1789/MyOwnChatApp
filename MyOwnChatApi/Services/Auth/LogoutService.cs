using MyOwnChatApi.Context;

namespace MyOwnChatApi.Services.Auth
{
    public class LogoutService: ILogoutService
    {
        private readonly ILogger<LogoutService> _logger;
        private readonly MyContext _db;

        public LogoutService(ILogger<LogoutService> logger, MyContext db)
        {
            _logger = logger;
            _db = db;
        }

        public async Task LogoutAsync(int userId)
        {
            _logger.LogInformation("UserId={UserId}のログアウト処理を開始します", userId);

            var user = await _db.Users
                .FindAsync(userId);

            if(user == null)
            {
                _logger.LogWarning("UserId={UserId}のユーザーが見つかりませんでした", userId);
                return;
            }

            // RefreshTokenの無効化
            user.RefreshTokenHash = null;
            user.RefreshTokenExpiresAt = null;

            await _db.SaveChangesAsync();

            _logger.LogInformation("UserId={UserId} のログアウト処理が完了しました", userId);
        }
    }
}
