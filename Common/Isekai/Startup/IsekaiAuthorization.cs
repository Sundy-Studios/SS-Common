namespace Common.Isekai.Startup;

using System;
using System.Linq;
using System.Reflection;
using Common.Isekai.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;

internal static class IsekaiAuthorization
{
    public static void ApplyAuthorization(RouteHandlerBuilder builder, Type iface, MethodInfo method)
    {
        if (method.IsDefined(typeof(IsekaiAllowAnonymousAttribute), true) ||
            iface.IsDefined(typeof(IsekaiAllowAnonymousAttribute), true))
        {
            builder.AllowAnonymous();
            return;
        }

        var authorizeAttributes = iface.GetCustomAttributes<IsekaiAuthorizeAttribute>(true)
            .Concat(method.GetCustomAttributes<IsekaiAuthorizeAttribute>(true))
            .ToArray();

        if (authorizeAttributes.Length == 0)
        {
            return;
        }

        foreach (var attr in authorizeAttributes)
        {
            var policy = new AuthorizationPolicyBuilder();
            policy.RequireAuthenticatedUser();

            if (!string.IsNullOrWhiteSpace(attr.Policy))
            {
                policy.RequireClaim("policy", attr.Policy);
            }

            if (!string.IsNullOrWhiteSpace(attr.Roles))
            {
                policy.RequireRole(attr.Roles.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
            }

            builder.RequireAuthorization(policy.Build());
        }
    }
}
