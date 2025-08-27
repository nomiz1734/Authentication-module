using Authentication_module.Models;

namespace Authentication_module.Services
{
    public interface ITokenService
    {
        string CreateAccessToken(User user);
        string CreateRefreshToken();
        string HashToken(string token);
    }
}
