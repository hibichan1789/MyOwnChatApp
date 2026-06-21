namespace MyOwnChatApi.Services.Auth
{
    public interface IEmailVerificationService
    {
        Task VerifyEmailAsync(string token);
    }
}
