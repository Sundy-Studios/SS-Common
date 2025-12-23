namespace Common.Proxy;

using System;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using Common.Attributes.Isekai;

public class IsekaiProxy<T> : DispatchProxy where T : class
{
    public HttpClient HttpClient { get; set; } = null!;
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
    {
        ArgumentNullException.ThrowIfNull(targetMethod);

        // Get path from IsekaiPathAttribute
        var pathAttr = targetMethod.GetCustomAttribute<IsekaiPathAttribute>() ?? throw new InvalidOperationException($"Method {targetMethod.Name} must have IsekaiPathAttribute");

        var path = pathAttr.Path;

        // Determine timeout
        var timeoutAttr = targetMethod.GetCustomAttribute<IsekaiMethodTimeoutAttribute>();
        var timeout = timeoutAttr?.Timeout ?? IsekaiMethodTimeout.Medium;

        var cancellation = timeout == IsekaiMethodTimeout.None
            ? default
            : new CancellationTokenSource((int)timeout).Token;

        // Assume return type is Task<T>
        var returnType = targetMethod.ReturnType;
        if (!returnType.IsGenericType || returnType.GetGenericTypeDefinition() != typeof(Task<>))
        {
            throw new NotSupportedException("Only async Task<T> methods are supported");
        }

        var resultType = returnType.GenericTypeArguments[0];

        // Make a basic GET request (could extend for POST)
        var httpResponse = HttpClient.GetAsync(path, cancellation).GetAwaiter().GetResult();
        httpResponse.EnsureSuccessStatusCode();
        var json = httpResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

        // Deserialize into return type
        var deserialized = JsonSerializer.Deserialize(json, resultType, _jsonOptions);
        return Activator.CreateInstance(typeof(Task<>).MakeGenericType(resultType), deserialized);
    }
}
