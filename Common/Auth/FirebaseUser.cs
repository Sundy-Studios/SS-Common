namespace Common.Auth;

using System.Security.Claims;

public static class FirebaseUser
{
    public static string? UserId(this ClaimsPrincipal user) => user.FindFirst("user_id")?.Value;

    public static string? Email(this ClaimsPrincipal user) => user.FindFirst(ClaimTypes.Email)?.Value;
}
