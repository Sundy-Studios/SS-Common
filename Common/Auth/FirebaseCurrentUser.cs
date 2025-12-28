namespace Common.Auth;

using System.Security.Claims;
using Microsoft.AspNetCore.Http;

public class FirebaseCurrentUser(IHttpContextAccessor contextAccessor) : ICurrentUser
{
    private readonly IHttpContextAccessor _contextAccessor = contextAccessor;

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
        User?.Claims?.ToArray() ?? [];

    private string? GetClaim(string type) => User?.Claims.FirstOrDefault(c => c.Type == type)?.Value;
}
