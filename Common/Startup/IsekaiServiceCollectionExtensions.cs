namespace Common.Startup;

using System.Reflection;
using Common.Attributes.Isekai;
using Common.Services.Isekai;
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
        assembly ??= Assembly.GetCallingAssembly();

        // Find all IIsekaiService interfaces
        var serviceInterfaces = assembly.GetTypes()
            .Where(t => t.IsInterface && typeof(IIsekaiService).IsAssignableFrom(t) && t != typeof(IIsekaiService));

        foreach (var iface in serviceInterfaces)
        {
            // Find a concrete class implementing this interface
            var impl = assembly.GetTypes()
                .FirstOrDefault(t => iface.IsAssignableFrom(t) && t.IsClass && !t.IsAbstract) ?? throw new InvalidOperationException($"No implementation found for {iface.FullName}");

            services.AddScoped(iface, impl);
        }

        return services;
    }

    /// <summary>
    /// Maps all IIsekaiService public methods with IsekaiPathAttribute as minimal API endpoints.
    /// </summary>
    public static WebApplication MapIsekaiEndpoints(this WebApplication app, Assembly? assembly = null)
    {
        assembly ??= Assembly.GetCallingAssembly();
        var serviceInterfaces = assembly.GetTypes()
            .Where(t => t.IsInterface && typeof(IIsekaiService).IsAssignableFrom(t) && t != typeof(IIsekaiService));

        foreach (var iface in serviceInterfaces)
        {
            var impl = assembly.GetTypes()
                .FirstOrDefault(t => iface.IsAssignableFrom(t) && t.IsClass && !t.IsAbstract);

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

    private static void ApplyAuthorization(
        RouteHandlerBuilder builder,
        Type iface,
        MethodInfo method)
    {
        // // AllowAnonymous always wins
        // if (method.IsDefined(typeof(AllowAnonymousAttribute), true))
        // {
        //     builder.AllowAnonymous();
        //     return;
        // }

        // var authorizeAttributes =
        //     iface.GetCustomAttributes<AuthorizeAttribute>(true)
        //          .Concat(method.GetCustomAttributes<AuthorizeAttribute>(true))
        //          .ToArray();

        // if (authorizeAttributes.Length == 0)
        //     return;

        // builder.RequireAuthorization(authorizeAttributes);
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
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Results.Conflict(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            // fallback for unhandled exceptions
            return Results.Problem(ex.Message);
        }
    }

}
