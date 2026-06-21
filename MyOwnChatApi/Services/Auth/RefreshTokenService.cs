using Microsoft.EntityFrameworkCore;
using MyOwnChatApi.Context;
using MyOwnChatApi.Domain.DTOs.Auth;

namespace MyOwnChatApi.Services.Auth
{
    public class RefreshTokenService: IRefreshTokenService
    {
        private readonly ILogger<RefreshTokenService> _logger;
        private readonly MyContext _db;
        private readonly ITokenService _tokenService;
        
        public RefreshTokenService(
            ILogger<RefreshTokenService> logger,
            MyContext db,
            ITokenService tokenService
            )
        { 
            _logger = logger;
            _db = db;
            _tokenService = tokenService;
        }

        public async Task<LoginResponseDto> RefreshTokenAsync(string refreshToken)
        {
            _logger.LogInformation("RefreshTokenの検証を開始します");

            var refreshTokenHash = SecurityService.HashToken(refreshToken);

            // refreshTokenHashを用いてDBからユーザー検索
            var user = await _db.Users
                .FirstOrDefaultAsync(u => u.RefreshTokenHash == refreshTokenHash);

            if (user == null)
            {
                throw new InvalidOperationException("無効なリフレッシュトークンです");
            }

            // refreshTokenの有効期限の検証
            if(user.RefreshTokenExpiresAt == null||
                user.RefreshTokenExpiresAt < DateTimeOffset.UtcNow
                )
            {
                throw new InvalidOperationException("リフレッシュトークンの有効期限が切れています");
            }

            // 新しいTokenの生成
            var newAccessToken = _tokenService.GenerateJwt(user);
            var newRefreshToken = _tokenService.GenerateRefreshToken();

            user.RefreshTokenHash = newRefreshToken.hash;
            user.RefreshTokenExpiresAt = newRefreshToken.expiresAt;
            await _db.SaveChangesAsync();

            _logger.LogInformation("UserId={UserId} の RefreshToken 再発行が完了しました", user.Id);

            return new LoginResponseDto
            {
                AccessToken = newAccessToken,
                AccessTokenExpiresAt = DateTimeOffset.UtcNow.AddMinutes(30),
                RefreshToken = newRefreshToken.token,
                RefreshTokenExpiresAt = newRefreshToken.expiresAt,
            };
        }
    }
}
