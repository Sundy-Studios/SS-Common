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

                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        Console.WriteLine($"Auth failed: {context.Exception}");
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context =>
                    {
                        Console.WriteLine("Validated User: {User}", JsonSerializer.Serialize(context.Principal, new JsonSerializerOptions
                        {
                            WriteIndented = true
                        }));
                        return Task.CompletedTask;
                    }
                };
            });

        services.AddAuthorization();
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, FirebaseCurrentUser>();

        return services;
    }
}
