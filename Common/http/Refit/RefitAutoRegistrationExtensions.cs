namespace Common.Http.Refit;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Refit;
using System.Reflection;

public static class RefitAutoRegistrationExtensions
{
    public static IServiceCollection AddRefitClientsFromAssembly(
        this IServiceCollection services,
        IConfiguration configuration,
        Assembly assembly,
        string baseUrlConfigKey)
    {
        var baseUrl = configuration[baseUrlConfigKey]
            ?? throw new InvalidOperationException($"{baseUrlConfigKey} is not configured");

        var apiClients = assembly
            .GetTypes()
            .Where(t =>
                t.IsInterface &&
                !t.IsGenericType &&
                typeof(IApiClient).IsAssignableFrom(t));

        foreach (var client in apiClients)
        {
            services
                .AddRefitClient(client)
                .ConfigureHttpClient(c => c.BaseAddress = new Uri(baseUrl));
        }

        return services;
    }
}
