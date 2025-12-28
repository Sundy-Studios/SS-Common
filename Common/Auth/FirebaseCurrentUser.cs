using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace Common.Auth;

internal sealed class FirebaseCurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _contextAccessor;

    public FirebaseCurrentUser(IHttpContextAccessor contextAccessor)
    {
        _contextAccessor = contextAccessor;
    }

    private ClaimsPrincipal? User =>
        _contextAccessor.HttpContext?.User;

    public bool IsAuthenticated =>
        User?.Identity?.IsAuthenticated == true;

    public string? UserId =>
        GetClaim("user_id")
        ?? GetClaim(ClaimTypes.NameIdentifier)
        ?? GetClaim("sub");

    public string? Email =>
        GetClaim(ClaimTypes.Email)
        ?? GetClaim("email");

    public bool EmailVerified =>
        bool.TryParse(GetClaim("email_verified"), out var verified) && verified;

    public string? Provider =>
        GetClaim("firebase.sign_in_provider")
        ?? GetClaim("sign_in_provider");

    public IReadOnlyCollection<Claim> Claims =>
        User?.Claims?.ToArray() ?? Array.Empty<Claim>();

    private string? GetClaim(string type)
    {
        return User?.Claims.FirstOrDefault(c => c.Type == type)?.Value;
    }
}
