using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Authentication_module.Data;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using System.Text.Json.Serialization;
using Authentication_module.Models;

namespace Authentication_module.Tests;

public class AuthFlowTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;
    private readonly JsonSerializerOptions _json =
    new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };
    public AuthFlowTests(ApiFactory factory) => _factory = factory;

    [Fact]
    public async Task Register_Login_Refresh_Home_Logout_end_to_end()
    {
        var client = _factory.CreateClientWithCookies();

        // 1) REGISTER (type = 0 => EndUser; nếu API bạn nhận chuỗi thì đổi thành "EndUser")
        var reg = new { username = "alice", email = "alice@test.com", password = "secret123", type = 0 };
        var r1 = await client.PostAsJsonAsync("/api/auth/register", reg, _json);
        Assert.Equal(HttpStatusCode.OK, r1.StatusCode);
        Assert.True(r1.Headers.TryGetValues("Set-Cookie", out var set1));
        Assert.Contains(set1!, c => c.StartsWith("refreshToken="));

        // 2) LOGIN sai
        var badLogin = new { usernameOrEmail = "alice", password = "wrong", rememberMe = false, type = 0 };
        var rBad = await client.PostAsJsonAsync("/api/auth/login", badLogin, _json);
        Assert.Equal(HttpStatusCode.Unauthorized, rBad.StatusCode);

        // 3) LOGIN đúng
        var goodLogin = new { usernameOrEmail = "alice", password = "secret123", rememberMe = true, type = 0 };
        var r2 = await client.PostAsJsonAsync("/api/auth/login", goodLogin, _json);
        Assert.Equal(HttpStatusCode.OK, r2.StatusCode);
        var loginBody = await r2.Content.ReadFromJsonAsync<AuthDto>(_json);
        Assert.False(string.IsNullOrWhiteSpace(loginBody!.AccessToken));

        // 4) REFRESH (cookie Secure đã có trong jar)
        var r3 = await client.PostAsync("/api/auth/refresh", null);
        Assert.Equal(HttpStatusCode.OK, r3.StatusCode);
        Assert.True(r3.Headers.TryGetValues("Set-Cookie", out var set2));
        Assert.Contains(set2!, c => c.StartsWith("refreshToken="));
        var refreshBody = await r3.Content.ReadFromJsonAsync<AuthDto>(_json);

        // 5) HOME (Authorize)
        var req = new HttpRequestMessage(HttpMethod.Get, "/api/home");
        req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", refreshBody!.AccessToken);
        var r4 = await client.SendAsync(req);
        Assert.Equal(HttpStatusCode.OK, r4.StatusCode);

        // 6) LOGOUT
        var reqLogout = new HttpRequestMessage(HttpMethod.Post, "/api/auth/logout");
        reqLogout.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", refreshBody.AccessToken);
        var r5 = await client.SendAsync(reqLogout);
        Assert.Equal(HttpStatusCode.NoContent, r5.StatusCode);

        // DB: refresh token đã xoá
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var u = db.Users.Single(x => x.Email == "alice@test.com");
        Assert.Null(u.RefreshTokenHash);
        Assert.Null(u.RefreshTokenExpiresAt);
    }

    private record AuthDto(string AccessToken, string Username, string Email, UserType Type);
}
