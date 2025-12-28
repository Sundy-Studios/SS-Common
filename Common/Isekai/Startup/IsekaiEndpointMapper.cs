namespace Common.Isekai.Startup;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Common.Exception.Contracts;
using Common.Exception.Models;
using Common.Isekai.Attributes;
using Common.Isekai.Services;
using Common.Utility;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

public static class IsekaiEndpointMapper
{
    public static WebApplication MapIsekaiEndpoints(this WebApplication app, Assembly? assembly = null)
    {
        var assemblies = assembly != null ? [assembly] : AppDomain.CurrentDomain.GetAssemblies();
        var allTypes = assemblies.SelectMany(ReflectionUtils.GetLoadableTypes);

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
                    IsekaiHttpMethod.Get => app.MapGet(pathAttr.Path, (IServiceProvider sp, HttpContext ctx) => InvokeIsekaiMethod(sp, iface, method, ctx)),
                    IsekaiHttpMethod.Post => app.MapPost(pathAttr.Path, (IServiceProvider sp, HttpContext ctx) => InvokeIsekaiMethod(sp, iface, method, ctx)),
                    IsekaiHttpMethod.Put => app.MapPut(pathAttr.Path, (IServiceProvider sp, HttpContext ctx) => InvokeIsekaiMethod(sp, iface, method, ctx)),
                    IsekaiHttpMethod.Delete => app.MapDelete(pathAttr.Path, (IServiceProvider sp, HttpContext ctx) => InvokeIsekaiMethod(sp, iface, method, ctx)),
                    IsekaiHttpMethod.Patch => throw new NotImplementedException(),
                    _ => throw new NotSupportedException($"HTTP method {pathAttr.Method} not supported")
                };

                builder.WithName($"{iface.Name}.{method.Name}");
                IsekaiAuthorization.ApplyAuthorization(builder, iface, method);
            }
        }

        return app;
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
    }
}
