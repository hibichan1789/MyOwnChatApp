using System.ComponentModel.DataAnnotations;

namespace MyOwnChatApi.Domain.DTOs.Auth
{
    public class RegisterRequestDto
    {
        [Required]
        public string UserName { get; set; } = string.Empty;
        [Required]
        [EmailAddress]
        public string Email {  get; set; } = string.Empty;
        [Required]
        public string Password { get; set; } = string.Empty;
    }
}
