namespace Common.Isekai.Startup;

using System;
using System.Linq;
using System.Reflection;
using Common.Isekai.Client;
using Common.Isekai.Services;
using Common.Utility;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Provides extension methods for registering Isekai services and clients.
/// </summary>
public static class IsekaiServiceCollectionExtensions
{
    /// <summary>
    /// Registers all IIsekaiService implementations found in the provided assembly or loaded assemblies,
    /// and adds automatic exception handling via <see cref="IsekaiExceptionStartupFilter"/>.
    /// </summary>
    public static IServiceCollection AddIsekai(this IServiceCollection services, Assembly? assembly = null)
    {
        var assemblies = assembly != null ? new[] { assembly } : AppDomain.CurrentDomain.GetAssemblies();
        var allTypes = assemblies.SelectMany(ReflectionUtils.GetLoadableTypes);

        var serviceInterfaces = allTypes
            .Where(t => t.IsInterface && typeof(IIsekaiService).IsAssignableFrom(t) && t != typeof(IIsekaiService));

        foreach (var iface in serviceInterfaces)
        {
            var impl = allTypes.FirstOrDefault(t => iface.IsAssignableFrom(t) && t.IsClass && !t.IsAbstract)
                       ?? throw new InvalidOperationException($"No implementation found for {iface.FullName}");

            services.AddScoped(iface, impl);
        }

        // Inject centralized exception handling for Isekai endpoints
        services.AddSingleton<IStartupFilter, IsekaiExceptionStartupFilter>();

        return services;
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
}

