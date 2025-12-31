namespace Common.Auth;

using System.Security.Claims;

public interface ICurrentUser
{
    public bool IsAuthenticated { get; }

    public string? UserId { get; }
    public string? Email { get; }

    public bool EmailVerified { get; }

    public string? Provider { get; }

    public IReadOnlyCollection<Claim> Claims { get; }
}
