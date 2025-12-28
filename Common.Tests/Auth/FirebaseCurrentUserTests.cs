namespace Common.Tests.Auth;

using System.Security.Claims;
using Common.Auth;
using Microsoft.AspNetCore.Http;

public class FirebaseCurrentUserTests
{
    private static FirebaseCurrentUser CreateUser(params Claim[] claims)
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
        var context = new DefaultHttpContext { User = principal };
        var accessor = new HttpContextAccessor { HttpContext = context };
        return new FirebaseCurrentUser(accessor);
    }

    [Theory]
    [InlineData("abc123", null, null)]
    [InlineData(null, "user456", null)]
    [InlineData(null, null, "sub789")]
    public void UserIdReturnsCorrectClaim(string? userIdClaim, string? nameIdClaim, string? subClaim)
    {
        var claims = new List<Claim>();
        if (userIdClaim != null)
        {
            claims.Add(new Claim("user_id", userIdClaim));
        }

        if (nameIdClaim != null)
        {
            claims.Add(new Claim(ClaimTypes.NameIdentifier, nameIdClaim));
        }

        if (subClaim != null)
        {
            claims.Add(new Claim("sub", subClaim));
        }

        var user = CreateUser([.. claims]);

        var expected = userIdClaim ?? nameIdClaim ?? subClaim;
        Assert.Equal(expected, user.UserId);
    }

    [Theory]
    [InlineData("me@example.com", null)]
    [InlineData(null, "other@example.com")]
    [InlineData(null, null)]
    public void EmailReturnsCorrectClaim(string? emailClaim, string? emailAltClaim)
    {
        var claims = new List<Claim>();
        if (emailClaim != null)
        {
            claims.Add(new Claim(ClaimTypes.Email, emailClaim));
        }

        if (emailAltClaim != null)
        {
            claims.Add(new Claim("email", emailAltClaim));
        }

        var user = CreateUser([.. claims]);

        var expected = emailClaim ?? emailAltClaim;
        Assert.Equal(expected, user.Email);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void IsAuthenticatedReturnsCorrectValue(bool isAuthenticated)
    {
        var identity = new ClaimsIdentity(isAuthenticated ? Array.Empty<Claim>() : null, "TestAuth");
        if (!isAuthenticated)
        {
            identity = new ClaimsIdentity(); // unauthenticated identity
        }

        var principal = new ClaimsPrincipal(identity);
        var context = new DefaultHttpContext { User = principal };
        var accessor = new HttpContextAccessor { HttpContext = context };
        var user = new FirebaseCurrentUser(accessor);

        Assert.Equal(isAuthenticated, user.IsAuthenticated);
    }

    [Theory]
    [InlineData("true", true)]
    [InlineData("false", false)]
    [InlineData(null, false)]
    [InlineData("notabool", false)]
    public void EmailVerifiedReturnsCorrectValue(string? claimValue, bool expected)
    {
        var claims = claimValue != null ? new[] { new Claim("email_verified", claimValue) } : [];
        var user = CreateUser(claims);
        Assert.Equal(expected, user.EmailVerified);
    }

    [Theory]
    [InlineData("password", null, "password")]
    [InlineData(null, "firebase", "firebase")]
    [InlineData(null, null, null)]
    public void ProviderReturnsCorrectValue(string? providerClaim, string? altClaim, string? expected)
    {
        var claims = new List<Claim>();
        if (providerClaim != null)
        {
            claims.Add(new Claim("sign_in_provider", providerClaim));
        }

        if (altClaim != null)
        {
            claims.Add(new Claim("firebase.sign_in_provider", altClaim));
        }

        var user = CreateUser([.. claims]);

        Assert.Equal(expected, user.Provider);
    }

    [Fact]
    public void ClaimsReturnsAllClaims()
    {
        var claims = new[]
        {
            new Claim("a", "1"),
            new Claim("b", "2")
        };
        var user = CreateUser(claims);

        Assert.Equal(2, user.Claims.Count);
        Assert.Contains(user.Claims, c => c.Type == "a" && c.Value == "1");
        Assert.Contains(user.Claims, c => c.Type == "b" && c.Value == "2");
    }
}
