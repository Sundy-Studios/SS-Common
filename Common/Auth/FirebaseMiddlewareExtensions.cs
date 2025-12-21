namespace Common.Auth;

using Microsoft.AspNetCore.Builder;

public static class FirebaseMiddlewareExtensions
{
    public static IApplicationBuilder UseFirebaseAuth(this IApplicationBuilder app)
    {
        app.UseAuthentication();
        app.UseAuthorization();
        return app;
    }
}
