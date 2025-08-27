using Authentication_module.Models;
using Authentication_module.Services;
using Xunit;

namespace Authentication_module.Tests;

public class AuthenticatorTests
{
    [Fact]
    public void Verify_returns_false_when_role_mismatch()
    {
        var user = new User { Username = "bob", PasswordHash = BCrypt.Net.BCrypt.HashPassword("p@ss"), Type = UserType.EndUser };
        var auth = new CompositeAuthenticator();

        var ok = auth.Verify(user, "p@ss", UserType.Admin, out var err);

        Assert.False(ok);
        Assert.Contains("Sai loại người dùng", err);
    }

    [Fact]
    public void Verify_returns_true_when_password_ok_and_role_match()
    {
        var user = new User { Username = "bob", PasswordHash = BCrypt.Net.BCrypt.HashPassword("p@ss"), Type = UserType.EndUser };
        var auth = new CompositeAuthenticator();

        var ok = auth.Verify(user, "p@ss", UserType.EndUser, out var err);

        Assert.True(ok);
        Assert.Null(err);
    }
}
