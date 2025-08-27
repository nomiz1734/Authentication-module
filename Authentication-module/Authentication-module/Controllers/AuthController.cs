using Authentication_module.Data;
using Authentication_module.DTOs;
using Authentication_module.Models;
using Authentication_module.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace Authentication_module.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly ITokenService _tokens;
        private readonly IAuthenticator _auth;
        private readonly IConfiguration _config;

        public AuthController(AppDbContext db, ITokenService tokens, IAuthenticator auth, IConfiguration config)
        {
            _db = db;
            _tokens = tokens;
            _auth = auth;
            _config = config;
        }

        [HttpPost("register")]
        public async Task<ActionResult<AuthResponse>> Register(RegisterRequest req)
        {
            req.Username = req.Username.Trim();
            req.Email = req.Email.Trim().ToLowerInvariant();

            if (await _db.Users.AnyAsync(u => u.Username == req.Username || u.Email == req.Email))
                return BadRequest("Username or Email already exists");

            var user = new User
            {
                Username = req.Username,
                Email = req.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password),
                Type = req.Type
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            var access = _tokens.CreateAccessToken(user);
            await SetRefreshCookieAsync(user, rememberMe: false);

            return new AuthResponse
            {
                AccessToken = access,
                Username = user.Username,
                Email = user.Email,
                Type = user.Type
            };
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponse>> Login(LoginRequest req)
        {
            var key = req.UsernameOrEmail.Trim().ToLowerInvariant();
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == key || u.Username.ToLower() == key);
            if (user is null) return Unauthorized("Incorrect login information");

            if (!_auth.Verify(user, req.Password, req.Type, out var error))
                return Unauthorized(error ?? "Incorrect login information");

            var access = _tokens.CreateAccessToken(user);
            await SetRefreshCookieAsync(user, rememberMe: req.RememberMe);

            return new AuthResponse
            {
                AccessToken = access,
                Username = user.Username,
                Email = user.Email,
                Type = user.Type
            };
        }

        [HttpPost("refresh")]
        public async Task<ActionResult<AuthResponse>> Refresh()
        {
            if (!Request.Cookies.TryGetValue("refreshToken", out var presented))
                return Unauthorized("There is no refresh token");

            var hash = _tokens.HashToken(presented);
            var user = await _db.Users.FirstOrDefaultAsync(u => u.RefreshTokenHash == hash);
            if (user is null || user.RefreshTokenExpiresAt is null || user.RefreshTokenExpiresAt < DateTimeOffset.UtcNow)
                return Unauthorized("Refresh token is invalid");

            var access = _tokens.CreateAccessToken(user);

            var standardDays = int.Parse(_config["Jwt:RefreshTokenDays"] ?? "7");
            var remainingDays = (user.RefreshTokenExpiresAt.Value - DateTimeOffset.UtcNow).TotalDays;
            var remembered = remainingDays > standardDays - 0.5;

            await SetRefreshCookieAsync(user, rememberMe: remembered);

            return new AuthResponse
            {
                AccessToken = access,
                Username = user.Username,
                Email = user.Email,
                Type = user.Type
            };
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                       ?? User.FindFirstValue("sub");
            if (Guid.TryParse(userId, out var id))
            {
                var user = await _db.Users.FindAsync(id);
                if (user != null)
                {
                    user.RefreshTokenHash = null;
                    user.RefreshTokenExpiresAt = null;
                    await _db.SaveChangesAsync();
                }
            }

            Response.Cookies.Delete("refreshToken", new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict
            });

            return NoContent();
        }

        private async Task SetRefreshCookieAsync(User user, bool rememberMe)
        {
            var token = _tokens.CreateRefreshToken();
            user.RefreshTokenHash = _tokens.HashToken(token);

            var days = rememberMe
                ? int.Parse(_config["Jwt:RememberMeRefreshDays"] ?? "30")
                : int.Parse(_config["Jwt:RefreshTokenDays"] ?? "7");

            user.RefreshTokenExpiresAt = DateTimeOffset.UtcNow.AddDays(days);
            await _db.SaveChangesAsync();

            var cookie = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = user.RefreshTokenExpiresAt
            };
            Response.Cookies.Append("refreshToken", token, cookie);
        }
    }
}
