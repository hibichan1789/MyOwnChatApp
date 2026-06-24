using Azure;
using Azure.Communication.Email;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyOwnChatApi.Context;
using MyOwnChatApi.Domain.DTOs.Auth;
using MyOwnChatApi.Domain.Models;

namespace MyOwnChatApi.Services.Auth
{
    public class RegisterService:IRegisterService
    {
        private readonly ILogger<RegisterService> _logger;
        private readonly MyContext _db;
        private readonly IConfiguration _config;
        private readonly PasswordHasher<User> _passwordHasher;
        

        public RegisterService(ILogger<RegisterService> logger, MyContext db, IConfiguration config)
        {
            _logger = logger;
            _db = db;
            _config = config;
            _passwordHasher = new PasswordHasher<User>();
        }

        public async Task RegisterAsync(RegisterRequestDto registerRequest)
        {
            _logger.LogInformation("Email={Email}のメールアドレスの重複検証を開始します",registerRequest.Email);
            var exists = await _db.Users.AnyAsync(u => u.Email == registerRequest.Email);
            if (exists)
            {
                throw new InvalidOperationException("このメールアドレスは既に登録されています");
            }

            var user = new User
            {
                UserName = registerRequest.UserName,
                Email = registerRequest.Email,
                CreatedAt = DateTimeOffset.UtcNow,
                IsVerified = false
            };
            // パスワードハッシュ
            user.PasswordHash = _passwordHasher.HashPassword(user, registerRequest.Password);
            // メール認証トークン生成
            var emailVerificationToken = SecurityService.GenerateRandomToken();
            user.EmailVerificationTokenHash = SecurityService.HashToken(emailVerificationToken);
            user.EmailVerificationTokenExpiresAt = DateTimeOffset.UtcNow.AddHours(1);

            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            _logger.LogInformation("UserId={UserId},Email={Email}のユーザーを登録しました",user.Id, user.Email);

            
            await SendVerificationEmailAsync(user.UserName, user.Email, emailVerificationToken);
        }

        private async Task SendVerificationEmailAsync(string userName, string email, string token)
        {
            var connectionString = _config["AzureCommunicationService:ConnectionString"];
            var senderAddress = _config["AzureCommunicationService:SenderAddress"];
            var emailClient = new EmailClient(connectionString);

            // TODO: フロントエンドの画面ができたらフロントエンドのURLを記載する
            // 今は今から作るコントローラのエンドポイントを記載
            var verifyUrl = $"https://proud-coast-04acbfb00.7.azurestaticapps.net/src/pages/verify/verify.html?token={Uri.EscapeDataString(token)}";

            var htmlBody = $@"
                <p>{userName}様</p>
                <p>自作チャットGPTにご登録いただきありがとうございます</p>
                <p>以下のリンクをクリックして本登録を完了してください</p>
                <p><a href=""{verifyUrl}"">メールアドレスを確認する</a></p>
                <p>このリンクは1時間で有効期限が切れます</p>
                ";

            var content = new EmailContent(subject: "【自作チャットGPT】メールアドレス確認のお願い")
            {
                Html = htmlBody
            };

            var message = new EmailMessage(
                    senderAddress: senderAddress,
                    recipientAddress: email,
                    content: content
                );

            await emailClient.SendAsync(WaitUntil.Completed, message);

            _logger.LogInformation("Email={Email}にメール認証リンクを送信しました", email);
        }
    }
}
