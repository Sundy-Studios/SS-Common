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
using Microsoft.AspNetCore.Hosting;
using System.Text.Json;

/// <summary>
/// Provides extension methods for registering and mapping IIsekaiService implementations
/// as minimal API endpoints.
/// </summary>
public static class IsekaiServiceCollectionExtensions
{
    /// <summary>
    /// Registers all IIsekaiService implementations found in the provided assembly or loaded assemblies,
    /// and adds automatic exception handling for unhandled exceptions (401/403 etc.).
    /// </summary>
    public static IServiceCollection AddIsekai(this IServiceCollection services, Assembly? assembly = null)
    {
        var assemblies = assembly != null ? [assembly] : AppDomain.CurrentDomain.GetAssemblies();
        var allTypes = assemblies.SelectMany(GetLoadableTypes);

        var serviceInterfaces = allTypes
            .Where(t => t.IsInterface && typeof(IIsekaiService).IsAssignableFrom(t) && t != typeof(IIsekaiService));

        foreach (var iface in serviceInterfaces)
        {
            var impl = allTypes.FirstOrDefault(t => iface.IsAssignableFrom(t) && t.IsClass && !t.IsAbstract)
                       ?? throw new ConflictException($"No implementation found for {iface.FullName}");

            services.AddScoped(iface, impl);
        }

        // automatically inject global exception handling via startup filter
        services.AddSingleton<IStartupFilter, IsekaiExceptionStartupFilter>();

        return services;
    }

    /// <summary>
    /// Maps all IIsekaiService public methods decorated with IsekaiPathAttribute as minimal API endpoints.
    /// </summary>
    public static WebApplication MapIsekaiEndpoints(this WebApplication app, Assembly? assembly = null)
    {
        var assemblies = assembly != null ? [assembly] : AppDomain.CurrentDomain.GetAssemblies();
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
            throw; // let global exception handler in startup filter catch it
        }
    }

    #endregion
}

/// <summary>
/// Startup filter that injects centralized error handling into the pipeline.
/// </summary>
internal sealed class IsekaiExceptionStartupFilter : IStartupFilter
{
    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) => app =>
    {
        app.Use(async (ctx, nextMiddleware) =>
        {
            Exception? exception = null;

            try
            {
                await nextMiddleware();
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            if (ctx.Response.HasStarted)
            {
                return;
            }

            var statusCode = exception switch
            {
                UnauthorizedException => StatusCodes.Status401Unauthorized,
                ForbiddenException => StatusCodes.Status403Forbidden,
                NotFoundException => StatusCodes.Status404NotFound,
                _ when exception != null => StatusCodes.Status500InternalServerError,
                _ => ctx.Response.StatusCode
            };

            if (statusCode < StatusCodes.Status400BadRequest)
            {
                return;
            }

            var message = ResolveMessage(statusCode, exception);

            await WriteError(ctx, statusCode, message, exception);
        });

        next(app);
    };

    private static string ResolveMessage(int statusCode, Exception? ex)
    {
        if (!string.IsNullOrWhiteSpace(ex?.Message))
        {
            return ex.Message;
        }

        return statusCode switch
        {
            StatusCodes.Status400BadRequest => "Bad request",
            StatusCodes.Status401Unauthorized => "Unauthorized",
            StatusCodes.Status403Forbidden => "Forbidden",
            StatusCodes.Status404NotFound => "Not found",
            StatusCodes.Status405MethodNotAllowed => "Method not allowed",
            StatusCodes.Status409Conflict => "Conflict",
            StatusCodes.Status422UnprocessableEntity => "Unprocessable entity",
            _ => "An unexpected error occurred"
        };
    }

    private static Task WriteError(
        HttpContext ctx,
        int statusCode,
        string message,
        Exception? ex)
    {
        ctx.Response.Clear();
        ctx.Response.StatusCode = statusCode;
        ctx.Response.ContentType = "application/json";

        List<string>? details = null;

#if DEBUG
        if (ex?.StackTrace != null)
        {
            details = new List<string> { ex.StackTrace };
        }
#endif

        var error = new ErrorResponse(
            Success: false,
            StatusCode: statusCode,
            Message: message,
            Details: details);

        return ctx.Response.WriteAsJsonAsync(error);
    }
}
