namespace Common.Isekai.Client;

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using Common.Isekai.Attributes;

internal sealed class IsekaiClientProxy : DispatchProxy
{
    private HttpClient _httpClient = null!;

    public void SetHttpClient(HttpClient httpClient) => _httpClient = httpClient;

    protected override object? Invoke(MethodInfo targetMethod, object?[] args)
    {
        var task = InvokeAsync(targetMethod, args);

        // Determine the return type
        var returnType = targetMethod.ReturnType;

        if (returnType == typeof(Task))
        {
            return task;
        }

        if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
        {
            var resultType = returnType.GenericTypeArguments[0];

            // Create Task<T> from Task<object?>
            var castMethod = typeof(IsekaiClientProxy)
                .GetMethod(nameof(CastTask), BindingFlags.NonPublic | BindingFlags.Instance)!
                .MakeGenericMethod(resultType);

            return castMethod.Invoke(this, [task])!;
        }

        throw new NotSupportedException("Only Task and Task<T> are supported");
    }

    private static async Task<T> CastTask<T>(Task<object?> task)
    {
        var result = await task.ConfigureAwait(false);
        return (T)result!;
    }


    private async Task<object?> InvokeAsync(MethodInfo method, object?[] args)
    {
        var pathAttr = method.GetCustomAttribute<IsekaiPathAttribute>()
                       ?? throw new InvalidOperationException($"{method.Name} missing IsekaiPathAttribute");

        var path = pathAttr.Path;

        var parameters = method.GetParameters();
        var query = new List<string>();
        object? body = null;

        for (var i = 0; i < parameters.Length; i++)
        {
            var p = parameters[i];
            var value = args[i];

            if (p.GetCustomAttribute<IsekaiFromRouteAttribute>() != null)
            {
                path = path.Replace("{" + p.Name + "}", Uri.EscapeDataString(value?.ToString() ?? ""));
                continue;
            }

            if (p.GetCustomAttribute<IsekaiFromQueryAttribute>() != null)
            {
                if (value != null)
                {
                    var props = value.GetType().GetProperties();
                    foreach (var prop in props)
                    {
                        var v = prop.GetValue(value);

                        // Only add to query if not null and, for value types, not default
                        if (v != null)
                        {
                            if (prop.PropertyType.IsValueType && Activator.CreateInstance(prop.PropertyType)!.Equals(v))
                            {
                                continue; // skip default value
                            }

                            query.Add($"{prop.Name}={Uri.EscapeDataString(v.ToString()!)}");
                        }
                    }
                }
                continue;
            }


            if (p.GetCustomAttribute<IsekaiFromBodyAttribute>() != null)
            {
                body = value;
                continue;
            }
        }

        if (query.Count > 0)
        {
            path += "?" + string.Join("&", query);
        }

        // Log the final request path for debugging
        Console.WriteLine($"ISEKAI → {method.Name} {path}");

        HttpResponseMessage response;

        if (pathAttr.Method == IsekaiHttpMethod.Get)
        {
            response = await _httpClient.GetAsync(path);
        }
        else if (pathAttr.Method == IsekaiHttpMethod.Post)
        {
            response = await _httpClient.PostAsJsonAsync(path, body);
        }
        else if (pathAttr.Method == IsekaiHttpMethod.Put)
        {
            response = await _httpClient.PutAsJsonAsync(path, body);
        }
        else if (pathAttr.Method == IsekaiHttpMethod.Delete)
        {
            response = await _httpClient.DeleteAsync(path);
        }
        else
        {
            throw new NotSupportedException($"HTTP method {pathAttr.Method} not supported");
        }

        response.EnsureSuccessStatusCode();

        if (method.ReturnType == typeof(Task))
        {
            return Task.CompletedTask;
        }

        if (method.ReturnType.IsGenericType &&
            method.ReturnType.GetGenericTypeDefinition() == typeof(Task<>))
        {
            var resultType = method.ReturnType.GenericTypeArguments[0];
            var result = await response.Content.ReadFromJsonAsync(resultType, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            return result;
        }

        return null;
    }
}
