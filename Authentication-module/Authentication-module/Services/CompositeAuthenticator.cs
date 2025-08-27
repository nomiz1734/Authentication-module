using Authentication_module.Models;

namespace Authentication_module.Services
{
    public class CompositeAuthenticator : IAuthenticator
    {
        public bool Verify(User user, string password, UserType? requestedType, out string? error)
        {
            error = null;
            if (requestedType.HasValue && requestedType.Value != user.Type)
            {
                error = "Sai loại người dùng cho tài khoản này";
                return false;
            }
            return BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
        }
    }
}
