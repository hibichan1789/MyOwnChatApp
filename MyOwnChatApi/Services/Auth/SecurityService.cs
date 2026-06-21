using System.Security.Cryptography;
using System.Text;

namespace MyOwnChatApi.Services.Auth
{
    public static class SecurityService
    {
        public static string GenerateRandomToken(int length = 64)
        {
            var bytes = new byte[length];
            // 強力な乱数生成
            RandomNumberGenerator.Fill(bytes);
            // 生成されたバイナリを文字列に変換して返す
            return Convert.ToBase64String(bytes);
        }
        public static string HashToken(string token)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(token);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}
