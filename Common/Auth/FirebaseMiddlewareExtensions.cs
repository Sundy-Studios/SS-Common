using Microsoft.AspNetCore.Builder;

namespace Common.Auth
{
    public static class FirebaseMiddlewareExtensions
    {
        public static IApplicationBuilder UseFirebaseAuth(this IApplicationBuilder app)
        {
            app.UseAuthentication();
            app.UseAuthorization();
            return app;
        }
    }
}
