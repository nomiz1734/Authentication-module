using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using Authentication_module.Models;
using Authentication_module.Services;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Authentication_module.Tests;

public class TokenServiceTests
{
    private readonly IConfiguration _cfg;
    private readonly TokenService _svc;

    public TokenServiceTests()
    {
        var dict = new Dictionary<string, string?>
        {
            ["Jwt:Key"] = new string('k', 80),
            ["Jwt:Issuer"] = "TestIssuer",
            ["Jwt:Audience"] = "TestAudience",
            ["Jwt:AccessTokenMinutes"] = "10",
        };
        _cfg = new ConfigurationBuilder().AddInMemoryCollection(dict).Build();
        _svc = new TokenService(_cfg);
    }

    [Fact]
    public void CreateAccessToken_has_expected_claims_and_exp()
    {
        var user = new User { Id = Guid.NewGuid(), Email = "a@b.com", Username = "alice", Type = UserType.Admin };
        var jwt = _svc.CreateAccessToken(user);
        Assert.False(string.IsNullOrWhiteSpace(jwt));

        var token = new JwtSecurityTokenHandler().ReadJwtToken(jwt);
        Assert.Equal("TestIssuer", token.Issuer);
        Assert.Contains("TestAudience", token.Audiences);
        Assert.Contains(token.Claims, c => c.Type == ClaimTypes.Role && c.Value == "Admin");
        Assert.Contains(token.Claims, c => c.Type == JwtRegisteredClaimNames.Email && c.Value == "a@b.com");
        Assert.True(token.ValidTo > DateTime.UtcNow.AddMinutes(8));
    }

    [Fact]
    public void CreateRefreshToken_is_random_and_long_enough()
    {
        var a = _svc.CreateRefreshToken();
        var b = _svc.CreateRefreshToken();
        Assert.False(string.IsNullOrWhiteSpace(a));
        Assert.NotEqual(a, b);
        Assert.True(Convert.FromBase64String(a).Length >= 48);
    }

    [Fact]
    public void HashToken_is_deterministic_and_hex()
    {
        var h1 = _svc.HashToken("sample");
        var h2 = _svc.HashToken("sample");
        Assert.Equal(h1, h2);
        Assert.Matches("^[0-9A-F]+$", h1);
        Assert.Equal(64, h1.Length);
    }
}
