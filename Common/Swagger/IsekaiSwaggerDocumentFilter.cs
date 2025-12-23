namespace Common.Swagger;

using System.Reflection;
using Common.Attributes.Isekai;
using Common.Services.Isekai;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

public class IsekaiSwaggerDocumentFilter(Assembly clientAssembly) : IDocumentFilter
{
    private readonly Assembly _clientAssembly = clientAssembly;

    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        // Find all interfaces implementing IIsekaiService
        var isekaiTypes = _clientAssembly
            .GetTypes()
            .Where(t => t.IsInterface && typeof(IIsekaiService).IsAssignableFrom(t) && t != typeof(IIsekaiService));

        foreach (var type in isekaiTypes)
        {
            foreach (var method in type.GetMethods())
            {
                var pathAttr = method.GetCustomAttribute<IsekaiPathAttribute>();
                if (pathAttr == null)
                {
                    continue;
                }

                var path = pathAttr.Path.StartsWith("/") ? pathAttr.Path : "/" + pathAttr.Path;

                // Only add GET endpoints here; can extend to support POST/PUT/etc.
                swaggerDoc.Paths[path] = new OpenApiPathItem
                {
                    Operations =
                    {
                        [OperationType.Get] = new OpenApiOperation
                        {
                            Summary = $"Isekai: {type.Name}.{method.Name}",
                            Responses =
                            {
                                ["200"] = new OpenApiResponse { Description = "Success" }
                            }
                        }
                    }
                };
            }
        }
    }
}
