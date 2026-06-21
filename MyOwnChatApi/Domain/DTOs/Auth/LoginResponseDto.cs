namespace MyOwnChatApi.Domain.DTOs.Auth
{
    public class LoginResponseDto
    {
        // RefreshTokenはCookieで入れるためレスポンスボディには含めない
        public string AccessToken { get; set; } = string.Empty;
        public DateTimeOffset AccessTokenExpiresAt { get; set; }

        // ContorollerのためにRefreshTokenのプロパティを作る
        public string RefreshToken { get; set; } = string.Empty;
        public DateTimeOffset RefreshTokenExpiresAt { get; set; }
    }
}
