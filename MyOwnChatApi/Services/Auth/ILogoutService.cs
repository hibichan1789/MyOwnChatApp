namespace MyOwnChatApi.Services.Auth
{
    public interface ILogoutService
    {
        Task LogoutAsync(int userId);
    }
}
