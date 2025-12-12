using System.Security.Claims;

namespace Common.Auth
{
    public static class FirebaseUser
    {
        public static string UserId(this ClaimsPrincipal user)
        {
            return user.FindFirst("user_id")?.Value;
        }

        public static string Email(this ClaimsPrincipal user)
        {
            return user.FindFirst(ClaimTypes.Email)?.Value;
        }
    }
}
