using MyOwnChatApi.Domain.Models;

namespace MyOwnChatApi.Services.Auth
{
    public interface ITokenService
    {
        string GenerateJwt(User user);
        (string token, string hash, DateTimeOffset expiresAt) GenerateRefreshToken();
    }
}
