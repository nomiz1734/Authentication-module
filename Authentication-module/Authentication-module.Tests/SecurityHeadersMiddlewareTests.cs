using Authentication_module.Utils;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace Authentication_module.Tests;

public class SecurityHeadersMiddlewareTests
{
    [Fact]
    public async Task Middleware_adds_expected_headers()
    {
        RequestDelegate next = _ => Task.CompletedTask;
        var mw = new SecurityHeadersMiddleware(next);

        var ctx = new DefaultHttpContext();
        await mw.Invoke(ctx);

        var h = ctx.Response.Headers;
        Assert.Equal("DENY", h["X-Frame-Options"].ToString());
        Assert.Equal("nosniff", h["X-Content-Type-Options"].ToString());
        Assert.Equal("no-referrer", h["Referrer-Policy"].ToString());
        Assert.Contains("default-src 'self'", h["Content-Security-Policy"].ToString());
    }
}
