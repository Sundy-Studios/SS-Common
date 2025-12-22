namespace Common.Tests.Auth;

using System.Security.Claims;
using Common.Auth;

public class FirebaseUserTests
{
    [Fact]
    public void UserIdReturnsClaimValue()
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity([new Claim("user_id", "abc123")]));

        var result = principal.UserId();

        Assert.Equal("abc123", result);
    }

    [Fact]
    public void UserIdWhenMissingReturnsNull()
    {
        var principal = new ClaimsPrincipal();

        var result = principal.UserId();

        Assert.Null(result);
    }

    [Fact]
    public void EmailReturnsClaimValue()
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Email, "me@example.com")]));

        var result = principal.Email();

        Assert.Equal("me@example.com", result);
    }

    [Fact]
    public void EmailWhenMissingReturnsNull()
    {
        var principal = new ClaimsPrincipal();

        var result = principal.Email();

        Assert.Null(result);
    }
}
