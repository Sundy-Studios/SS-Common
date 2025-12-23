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

                var path = pathAttr.Path.StartsWith('/')
                    ? pathAttr.Path
                    : "/" + pathAttr.Path;

                // Map your enum to Swagger OperationType
                var operationType = pathAttr.Method switch
                {
                    IsekaiHttpMethod.Get => OperationType.Get,
                    IsekaiHttpMethod.Post => OperationType.Post,
                    IsekaiHttpMethod.Put => OperationType.Put,
                    IsekaiHttpMethod.Delete => OperationType.Delete,
                    IsekaiHttpMethod.Patch => OperationType.Patch,
                    _ => OperationType.Get
                };

                if (!swaggerDoc.Paths.TryGetValue(path, out var value))
                {
                    value = new OpenApiPathItem();
                    swaggerDoc.Paths[path] = value;
                }

                value.Operations[operationType] = new OpenApiOperation
                {
                    Summary = $"Isekai: {type.Name}.{method.Name}",
                    Responses =
                    {
                        ["200"] = new OpenApiResponse { Description = "Success" }
                    }
                };
            }
        }
    }
}
