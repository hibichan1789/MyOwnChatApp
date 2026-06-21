using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyOwnChatApi.Context;
using MyOwnChatApi.Domain.DTOs.Auth;
using MyOwnChatApi.Domain.Models;

namespace MyOwnChatApi.Services.Auth
{
    public class LoginService:ILoginService
    {
        private readonly ILogger<LoginService> _logger;
        private readonly MyContext _db;
        private readonly PasswordHasher<User> _passwordHasher;
        private readonly ITokenService _tokenService;

        public LoginService(
            ILogger<LoginService> logger,
            MyContext db,
            ITokenService tokenService)
        {
            _logger = logger;
            _db = db;
            _passwordHasher = new PasswordHasher<User>();
            _tokenService = tokenService;
        }

        public async Task<LoginResponseDto> LoginAsync(LoginRequestDto loginRequest)
        {
            _logger.LogInformation("Email={Email}のログイン処理を開始します", loginRequest.Email);

            // Emailで検索
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email ==  loginRequest.Email);
            if(user == null)
            {
                throw new InvalidOperationException("メールアドレスまたはパスワードが正しくありません");
            }

            // 本登録してるか確認
            if (!user.IsVerified)
            {
                throw new InvalidOperationException("本登録されていません");
            }

            // パスワード検証
            var passwordValidationResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, loginRequest.Password);
            if (passwordValidationResult == PasswordVerificationResult.Failed)
            {
                throw new InvalidOperationException("メールアドレスまたはパスワードが正しくありません");
            }

            var accessToken = _tokenService.GenerateJwt(user);
            var refreshToken = _tokenService.GenerateRefreshToken();

            // refreshTokenHash,ExpiresAtをDBに保存
            user.RefreshTokenHash = refreshToken.hash;
            user.RefreshTokenExpiresAt = refreshToken.expiresAt;
            await _db.SaveChangesAsync();

            _logger.LogInformation("UserId={UserId} のログイン処理が完了しました", user.Id);
            return new LoginResponseDto
            {
                AccessToken = accessToken,
                AccessTokenExpiresAt = DateTimeOffset.UtcNow.AddMinutes(30),
                RefreshToken = refreshToken.token,
                RefreshTokenExpiresAt = refreshToken.expiresAt
            };
        }
    }
}
