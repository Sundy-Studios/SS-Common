using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Authentication;

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
