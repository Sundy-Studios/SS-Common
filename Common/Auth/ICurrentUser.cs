using System.Security.Claims;

namespace Common.Auth;

public interface ICurrentUser
{
    bool IsAuthenticated { get; }

    string? UserId { get; }
    string? Email { get; }

    bool EmailVerified { get; }

    string? Provider { get; }

    IReadOnlyCollection<Claim> Claims { get; }
}
