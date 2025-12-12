using System.Security.Claims;
using Common.Auth;

namespace Common.Tests.Auth;

public class FirebaseUserTests
{
    [Fact]
    public void UserId_ReturnsClaimValue()
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("user_id", "abc123") }));

        var result = principal.UserId();

        Assert.Equal("abc123", result);
    }

    [Fact]
    public void UserId_WhenMissing_ReturnsNull()
    {
        var principal = new ClaimsPrincipal();

        var result = principal.UserId();

        Assert.Null(result);
    }

    [Fact]
    public void Email_ReturnsClaimValue()
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Email, "me@example.com") }));

        var result = principal.Email();

        Assert.Equal("me@example.com", result);
    }

    [Fact]
    public void Email_WhenMissing_ReturnsNull()
    {
        var principal = new ClaimsPrincipal();

        var result = principal.Email();

        Assert.Null(result);
    }
}
