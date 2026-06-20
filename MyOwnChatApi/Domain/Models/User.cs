using System.ComponentModel.DataAnnotations;

namespace MyOwnChatApi.Domain.Models
{
    public class User
    {
        public int Id { get; set; }

        [StringLength(maximumLength: 255)]
        public string UserName { get; set; } = string.Empty;

        [StringLength(maximumLength: 255)]
        public string Email { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;

        public DateTimeOffset CreatedAt { get; set; }
        public bool IsVerified { get; set; }
        public string? RefreshTokenHash { get; set; }

        public DateTimeOffset? RefreshTokenExpiresAt { get; set; }
        public string? EmailVerificationTokenHash { get; set; }
        public DateTimeOffset? EmailVerificationTokenExpiresAt { get; set; }
    }
}
