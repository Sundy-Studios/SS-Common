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

public static class IsekaiServiceCollectionExtensions
{
    /// <summary>
    /// Registers all IIsekaiService implementations found in the assembly.
    /// </summary>
    public static IServiceCollection AddIsekai(this IServiceCollection services, Assembly? assembly = null)
    {
        var assemblies = assembly != null
            ? [assembly]
            : AppDomain.CurrentDomain.GetAssemblies();

        var allTypes = assemblies.SelectMany(GetLoadableTypes);

        var serviceInterfaces = allTypes
            .Where(t =>
                t.IsInterface &&
                typeof(IIsekaiService).IsAssignableFrom(t) &&
                t != typeof(IIsekaiService));

        foreach (var iface in serviceInterfaces)
        {
            // Find a concrete class implementing this interface
            var impl = allTypes.FirstOrDefault(t =>
                iface.IsAssignableFrom(t) &&
                t.IsClass &&
                !t.IsAbstract)
                ?? throw new ConflictException($"No implementation found for {iface.FullName}");

            services.AddScoped(iface, impl);
        }

        return services;
    }

    /// <summary>
    /// Maps all IIsekaiService public methods with IsekaiPathAttribute as minimal API endpoints.
    /// </summary>
    public static WebApplication MapIsekaiEndpoints(this WebApplication app, Assembly? assembly = null)
    {
        var assemblies = assembly != null
            ? [assembly]
            : AppDomain.CurrentDomain.GetAssemblies();

        var allTypes = assemblies.SelectMany(GetLoadableTypes);

        var serviceInterfaces = allTypes
            .Where(t =>
                t.IsInterface &&
                typeof(IIsekaiService).IsAssignableFrom(t) &&
                t != typeof(IIsekaiService));

        foreach (var iface in serviceInterfaces)
        {
            var impl = allTypes.FirstOrDefault(t =>
                iface.IsAssignableFrom(t) &&
                t.IsClass &&
                !t.IsAbstract);

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
                    IsekaiHttpMethod.Get => app.MapGet(
                        pathAttr.Path,
                        (IServiceProvider sp, HttpContext ctx) =>
                            InvokeIsekaiMethod(sp, iface, method, ctx)),

                    IsekaiHttpMethod.Post => app.MapPost(
                        pathAttr.Path,
                        (IServiceProvider sp, HttpContext ctx) =>
                            InvokeIsekaiMethod(sp, iface, method, ctx)),

                    IsekaiHttpMethod.Put => app.MapPut(
                        pathAttr.Path,
                        (IServiceProvider sp, HttpContext ctx) =>
                            InvokeIsekaiMethod(sp, iface, method, ctx)),

                    IsekaiHttpMethod.Delete => app.MapDelete(
                        pathAttr.Path,
                        (IServiceProvider sp, HttpContext ctx) =>
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

    public static IServiceCollection AddIsekaiClient<T>(this IServiceCollection services, HttpClient httpClient)
        where T : class, IIsekaiService   // <-- Add this constraint
    {
        services.AddSingleton(sp =>
            // Pass the HttpClient instance to the proxy factory
            IsekaiClient.Create<T>(httpClient));

        return services;
    }

    private static void ApplyAuthorization(
        RouteHandlerBuilder builder,
        Type iface,
        MethodInfo method)
    {
        // AllowAnonymous always wins
        if (method.IsDefined(typeof(IsekaiAllowAnonymousAttribute), inherit: true) ||
            iface.IsDefined(typeof(IsekaiAllowAnonymousAttribute), inherit: true))
        {
            builder.AllowAnonymous();
            return;
        }

        var authorizeAttributes =
            iface.GetCustomAttributes<IsekaiAuthorizeAttribute>(true)
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
                policy.RequireRole(
                    attr.Roles
                        .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
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

    private static async Task<object?[]> BindParameters(
        MethodInfo method,
        HttpContext ctx)
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

                // Only Task<T> should produce a body
                if (method.ReturnType.IsGenericType &&
                    method.ReturnType.GetGenericTypeDefinition() == typeof(Task<>))
                {
                    var resultProperty = task.GetType().GetProperty("Result");
                    var value = resultProperty?.GetValue(task);

                    return value == null
                        ? Results.NoContent()
                        : Results.Ok(value);
                }

                // Task (no <T>) → 204
                return Results.NoContent();
            }


            // Synchronous method
            return result == null ? Results.NoContent() : Results.Ok(result);
        }
        catch (CommonHttpException ex)
        {
            return Results.Json(
                new ErrorResponse(
                    false,
                    (int)ex.StatusCode,
                    ex.Message ?? ex.StatusCode.ToString(),
                    ex.Details
                ),
                statusCode: (int)ex.StatusCode
            );
        }
        catch (Exception)
        {
            return Results.Json(
                new ErrorResponse(
                    false,
                    StatusCodes.Status500InternalServerError,
                    "Internal server error"
                ),
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

}
