namespace Common.Auth;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

public static class FirebaseAuthExtensions
{
    public static IServiceCollection AddFirebaseAuth(this IServiceCollection services, IConfiguration config)
    {
        var projectId = config["Firebase:ProjectId"];
        var authority = $"https://securetoken.google.com/{projectId}";
        var audience = projectId;

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.Authority = authority;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = authority,
                    ValidAudience = audience,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true
                };
            });

        services.AddAuthorization();
        services.AddScoped<ICurrentUser, FirebaseCurrentUser>();

        return services;
    }
}
