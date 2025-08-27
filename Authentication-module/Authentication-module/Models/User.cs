using System.ComponentModel.DataAnnotations;
namespace Authentication_module.Models
{
    public enum UserType
    {
        EndUser = 0,
        Admin = 1,
        Partner = 2
    }

    public class User
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [MaxLength(64)]
        public string Username { get; set; } = string.Empty;

        [MaxLength(255)]
        public string Email { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;

        public UserType Type { get; set; } = UserType.EndUser;

        public string? RefreshTokenHash { get; set; }
        public DateTimeOffset? RefreshTokenExpiresAt { get; set; }
    }
}
