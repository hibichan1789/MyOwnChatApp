using MyOwnChatApi.Domain.DTOs.Auth;

namespace MyOwnChatApi.Services.Auth
{
    public interface ILoginService
    {
        Task<LoginResponseDto> LoginAsync(LoginRequestDto loginRequest);
    }
}
