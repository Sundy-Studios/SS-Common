namespace Common.Isekai.Startup;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Common.Exception.Contracts;
using Common.Exception.Models;
using Common.Isekai.Attributes;
using Common.Isekai.Client;
using Common.Isekai.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Provides extension methods for registering and mapping IIsekaiService implementations
/// as minimal API endpoints.
/// </summary>
public static class IsekaiServiceCollectionExtensions
{
    /// <summary>
    /// Registers all IIsekaiService implementations found in the provided assembly or loaded assemblies.
    /// </summary>
    public static IServiceCollection AddIsekai(this IServiceCollection services, Assembly? assembly = null)
    {
        var assemblies = assembly != null
            ? [assembly]
            : AppDomain.CurrentDomain.GetAssemblies();

        var allTypes = assemblies.SelectMany(GetLoadableTypes);

        var serviceInterfaces = allTypes
            .Where(t => t.IsInterface && typeof(IIsekaiService).IsAssignableFrom(t) && t != typeof(IIsekaiService));

        foreach (var iface in serviceInterfaces)
        {
            var impl = allTypes.FirstOrDefault(t => iface.IsAssignableFrom(t) && t.IsClass && !t.IsAbstract)
                       ?? throw new ConflictException($"No implementation found for {iface.FullName}");

            services.AddScoped(iface, impl);
        }

        return services;
    }

    /// <summary>
    /// Maps all IIsekaiService public methods decorated with IsekaiPathAttribute as minimal API endpoints.
    /// </summary>
    public static WebApplication MapIsekaiEndpoints(this WebApplication app, Assembly? assembly = null)
    {
        var assemblies = assembly != null
            ? [assembly]
            : AppDomain.CurrentDomain.GetAssemblies();

        var allTypes = assemblies.SelectMany(GetLoadableTypes);

        var serviceInterfaces = allTypes
            .Where(t => t.IsInterface && typeof(IIsekaiService).IsAssignableFrom(t) && t != typeof(IIsekaiService));

        foreach (var iface in serviceInterfaces)
        {
            var impl = allTypes.FirstOrDefault(t => iface.IsAssignableFrom(t) && t.IsClass && !t.IsAbstract);
            if (impl == null)
            {
                continue;
            }

            foreach (var method in iface.GetMethods())
            {
                var pathAttr = method.GetCustomAttribute<IsekaiPathAttribute>();
                if (pathAttr == null)
                {
                    continue;
                }

                var builder = pathAttr.Method switch
                {
                    IsekaiHttpMethod.Get => app.MapGet(pathAttr.Path, (IServiceProvider sp, HttpContext ctx) =>
                        InvokeIsekaiMethod(sp, iface, method, ctx)),

                    IsekaiHttpMethod.Post => app.MapPost(pathAttr.Path, (IServiceProvider sp, HttpContext ctx) =>
                        InvokeIsekaiMethod(sp, iface, method, ctx)),

                    IsekaiHttpMethod.Put => app.MapPut(pathAttr.Path, (IServiceProvider sp, HttpContext ctx) =>
                        InvokeIsekaiMethod(sp, iface, method, ctx)),

                    IsekaiHttpMethod.Delete => app.MapDelete(pathAttr.Path, (IServiceProvider sp, HttpContext ctx) =>
                        InvokeIsekaiMethod(sp, iface, method, ctx)),

                    IsekaiHttpMethod.Patch => throw new NotImplementedException(),
                    _ => throw new NotSupportedException($"HTTP method {pathAttr.Method} not supported")
                };

                builder.WithName($"{iface.Name}.{method.Name}");
                ApplyAuthorization(builder, iface, method);
            }
        }

        return app;
    }

    /// <summary>
    /// Registers an Isekai client proxy for a given IIsekaiService interface.
    /// </summary>
    public static IServiceCollection AddIsekaiClient<T>(this IServiceCollection services, HttpClient httpClient)
        where T : class, IIsekaiService
    {
        services.AddSingleton(sp => IsekaiClient.Create<T>(httpClient));
        return services;
    }

    /// <summary>
    /// Middleware for automatically transforming 401/403 responses into ErrorResponse format.
    /// Call after UseAuthentication and UseAuthorization.
    /// </summary>
    public static IApplicationBuilder UseIsekaiErrorResponses(this IApplicationBuilder app)
    {
        app.Use(async (ctx, next) =>
        {
            await next();

            if (ctx.Response.HasStarted)
            {
                return;
            }

            if (ctx.Response.StatusCode is StatusCodes.Status401Unauthorized or
                StatusCodes.Status403Forbidden)
            {
                var message = ctx.Response.StatusCode == 401 ? "Unauthorized" : "Forbidden";

                ctx.Response.ContentType = "application/json";

                var error = new ErrorResponse(
                    Success: false,
                    StatusCode: ctx.Response.StatusCode,
                    Message: message
                );

                await ctx.Response.WriteAsJsonAsync(error);
            }
        });

        return app;
    }

    #region Private helpers

    private static void ApplyAuthorization(RouteHandlerBuilder builder, Type iface, MethodInfo method)
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

    private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            return ex.Types.Where(t => t != null)!;
        }
    }

    private static async Task<object?[]> BindParameters(MethodInfo method, HttpContext ctx)
    {
        var values = new List<object?>();

        foreach (var p in method.GetParameters())
        {
            if (p.GetCustomAttribute<IsekaiFromRouteAttribute>() != null)
            {
                values.Add(ctx.Request.RouteValues[p.Name!]?.ToString());
                continue;
            }

            if (p.GetCustomAttribute<IsekaiFromQueryAttribute>() != null)
            {
                var type = p.ParameterType;

                if (type.IsPrimitive || type == typeof(string))
                {
                    values.Add(Convert.ChangeType(ctx.Request.Query[p.Name!], type));
                    continue;
                }

                var obj = Activator.CreateInstance(type)!;
                foreach (var prop in type.GetProperties())
                {
                    if (ctx.Request.Query.TryGetValue(prop.Name, out var value))
                    {
                        prop.SetValue(obj, Convert.ChangeType(value.ToString(), prop.PropertyType));
                    }
                }

                values.Add(obj);
                continue;
            }

            if (p.GetCustomAttribute<IsekaiFromBodyAttribute>() != null)
            {
                values.Add(await ctx.Request.ReadFromJsonAsync(p.ParameterType));
                continue;
            }

            values.Add(null);
        }

        return [.. values];
    }

    private static async Task<IResult> InvokeIsekaiMethod(IServiceProvider sp, Type iface, MethodInfo method, HttpContext ctx)
    {
        try
        {
            var service = sp.GetRequiredService(iface);
            var parameters = await BindParameters(method, ctx);
            var result = method.Invoke(service, parameters);

            if (result is Task task)
            {
                await task.ConfigureAwait(false);

                if (method.ReturnType.IsGenericType && method.ReturnType.GetGenericTypeDefinition() == typeof(Task<>))
                {
                    var value = task.GetType().GetProperty("Result")?.GetValue(task);
                    return value == null ? Results.NoContent() : Results.Ok(value);
                }

                return Results.NoContent();
            }

            return result == null ? Results.NoContent() : Results.Ok(result);
        }
        catch (CommonHttpException ex)
        {
            return Results.Json(
                new ErrorResponse(false, (int)ex.StatusCode, ex.Message!, ex.Details),
                statusCode: (int)ex.StatusCode
            );
        }
        catch (Exception ex)
        {
            var details = new List<string> { ex.Message };
#if DEBUG
            details.Add(ex.StackTrace ?? string.Empty);
#endif
            return Results.Json(
                new ErrorResponse(false, StatusCodes.Status500InternalServerError, "Internal server error", details),
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    #endregion
}
