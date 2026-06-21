using MyOwnChatApi.Domain.DTOs.Auth;

namespace MyOwnChatApi.Services.Auth
{
    public interface IRefreshTokenService
    {
        Task<LoginResponseDto> RefreshTokenAsync(string refreshToken);
    }
}
