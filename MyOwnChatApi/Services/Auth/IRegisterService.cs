using MyOwnChatApi.Domain.DTOs.Auth;

namespace MyOwnChatApi.Services.Auth
{
    public interface IRegisterService
    {
        Task RegisterAsync(RegisterRequestDto registerRequest);
    }
}
