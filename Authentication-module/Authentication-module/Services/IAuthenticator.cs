using Authentication_module.Models;

namespace Authentication_module.Services
{
    public interface IAuthenticator
    {
        bool Verify(User user, string password, UserType? requestedType, out string? error);
    }
}
