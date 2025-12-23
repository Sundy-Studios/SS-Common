namespace Common.Startup;

using System.Reflection;
using Common.Services.Isekai;
using Microsoft.Extensions.DependencyInjection;

public static class IsekaiServiceCollectionExtensions
{
    /// <summary>
    /// Registers all IIsekaiService interfaces with the DI container.
    /// Replace the NotImplementedException with your proxy/factory as needed.
    /// </summary>
    public static IServiceCollection AddIsekai(this IServiceCollection services, Assembly? assembly = null)
    {
        assembly ??= Assembly.GetCallingAssembly();

        var isekaiTypes = assembly
            .GetTypes()
            .Where(t => t.IsInterface && typeof(IIsekaiService).IsAssignableFrom(t) && t != typeof(IIsekaiService));

        foreach (var type in isekaiTypes)
        {
            services.AddSingleton(type, _ =>
                // TODO: Replace with your proxy, mock, or real implementation
                throw new NotImplementedException(
                    $"No implementation registered for {type.FullName}"));
        }

        return services;
    }
}
