using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using MyOwnChatApi.Domain.Models;
using System.Security.Claims;
using System.Text;

namespace MyOwnChatApi.Services.Auth
{
    public class TokenService: ITokenService
    {
        private readonly ILogger<TokenService> _logger;
        private readonly IConfiguration _config;

        public TokenService(ILogger<TokenService> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;
        }

        public string GenerateJwt(User user)
        {
            _logger.LogInformation("UserId={UserId}のJWTの生成を開始します", user.Id);

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expires = DateTime.UtcNow.AddMinutes(30);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email)
            };

            var token = new JwtSecurityToken(
                    issuer: _config["Jwt:Issuer"],
                    audience: _config["Jwt:Audience"],
                    claims: claims,
                    expires: expires,
                    signingCredentials: creds
                );

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            _logger.LogInformation("UserId={UserId}のJWTの生成が完了しました",user.Id);

            return jwt;
        }

        public (string token, string hash, DateTimeOffset expiresAt) GenerateRefreshToken()
        {
            _logger.LogInformation("RefreshTokenの生成を開始します");

            var token = SecurityService.GenerateRandomToken();
            var hash = SecurityService.HashToken(token);
            var expiresAt = DateTimeOffset.UtcNow.AddDays(1);

            _logger.LogInformation("RefreshTokenの生成が完了しました");
            return (token, hash, expiresAt);
        }
    }
}
