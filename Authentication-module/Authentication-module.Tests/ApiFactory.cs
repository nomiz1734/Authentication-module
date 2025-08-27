using System.Net.Http.Headers;
using System.Text;
using Authentication_module.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Authentication_module.Tests
{
    public class ApiFactory : WebApplicationFactory<Program>
    {
        private SqliteConnection? _conn;

        private const string TestIssuer = "TestIssuer";
        private const string TestAudience = "TestAudience";
        private const string TestKey =
            "THIS_IS_A_LONG_UNIT_TEST_SECRET_KEY_1234567890_ABCDEFGHIJKLMNOPQRSTUVWXYZ_!";

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureAppConfiguration((ctx, cfg) =>
            {
                cfg.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Jwt:Key"] = TestKey,
                    ["Jwt:Issuer"] = TestIssuer,
                    ["Jwt:Audience"] = TestAudience,
                    ["Jwt:AccessTokenMinutes"] = "5",
                    ["Jwt:RefreshTokenDays"] = "7",
                    ["Jwt:RememberMeRefreshDays"] = "30",
                    ["Cors:Origins:0"] = "https://localhost"
                });
            });
            builder.ConfigureServices(services =>
            {
                _conn = new SqliteConnection("DataSource=:memory:");
                _conn.Open();

                var descriptor = services.Single(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                services.Remove(descriptor);
                services.AddDbContext<AppDbContext>(opt => opt.UseSqlite(_conn));
            });
            builder.ConfigureTestServices(services =>
            {
                var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TestKey));

                services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, o =>
                {
                    o.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = signingKey,
                        ValidateIssuer = true,
                        ValidIssuer = TestIssuer,
                        ValidateAudience = true,
                        ValidAudience = TestAudience,
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero
                    };
                });
            });
        }
        public HttpClient CreateClientWithCookies()
        {
            var client = CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false,
                HandleCookies = true,
                BaseAddress = new Uri("https://localhost")
            });

            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            return client;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            _conn?.Dispose(); 
        }
    }
}
