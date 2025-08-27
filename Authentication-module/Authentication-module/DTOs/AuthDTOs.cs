using Authentication_module.Models;
using System.ComponentModel.DataAnnotations;
namespace Authentication_module.DTOs
{
    public class LoginRequest
    {
        public string UsernameOrEmail { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool RememberMe { get; set; } = false;
        public UserType? Type { get; set; }
    }

    public class RegisterRequest
    {
        [MaxLength(64)]
        public string Username { get; set; } = string.Empty;

        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [MinLength(6)]
        public string Password { get; set; } = string.Empty;

        public UserType Type { get; set; } = UserType.EndUser;
    }

    public class AuthResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public UserType Type { get; set; }
    }
}
