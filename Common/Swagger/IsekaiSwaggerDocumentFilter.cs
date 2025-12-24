namespace Common.Swagger;

using System.Reflection;
using Common.Attributes.Isekai;
using Common.Services.Isekai;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

public class IsekaiSwaggerDocumentFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        // Find all interfaces implementing IIsekaiService
        var isekaiTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
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

                var httpMethod = pathAttr.Method switch
                {
                    IsekaiHttpMethod.Get => OperationType.Get,
                    IsekaiHttpMethod.Post => OperationType.Post,
                    IsekaiHttpMethod.Put => OperationType.Put,
                    IsekaiHttpMethod.Delete => OperationType.Delete,
                    IsekaiHttpMethod.Patch => OperationType.Patch,
                    _ => OperationType.Get
                };

                if (!swaggerDoc.Paths.TryGetValue(path, out var pathItem))
                {
                    pathItem = new OpenApiPathItem();
                    swaggerDoc.Paths[path] = pathItem;
                }

                var operation = new OpenApiOperation
                {
                    Summary = $"Isekai: {type.Name}.{method.Name}",
                    Responses =
                    {
                        ["200"] = new OpenApiResponse { Description = "Success" }
                    }
                };

                // Handle parameters
                foreach (var param in method.GetParameters())
                {
                    var fromQuery = param.GetCustomAttribute<IsekaiFromQueryAttribute>() != null;
                    var fromRoute = param.GetCustomAttribute<IsekaiFromRouteAttribute>() != null;
                    var fromBody = param.GetCustomAttribute<IsekaiFromBodyAttribute>() != null;

                    var name = param.Name ?? "param";

                    if (fromBody)
                    {
                        operation.RequestBody = new OpenApiRequestBody
                        {
                            Content =
                            {
                                ["application/json"] = new OpenApiMediaType
                                {
                                    Schema = context.SchemaGenerator.GenerateSchema(param.ParameterType, context.SchemaRepository)
                                }
                            },
                            Required = true
                        };
                    }
                    else if (fromQuery && param.ParameterType.IsClass && param.ParameterType != typeof(string))
                    {
                        // Flatten each property of the query object into its own parameter
                        foreach (var prop in param.ParameterType.GetProperties())
                        {
                            operation.Parameters.Add(new OpenApiParameter
                            {
                                Name = prop.Name,
                                In = ParameterLocation.Query,
                                Required = false, // mark as optional unless you add custom attribute
                                Schema = context.SchemaGenerator.GenerateSchema(prop.PropertyType, context.SchemaRepository)
                            });
                        }
                    }
                    else
                    {
                        // primitive or simple query/route param
                        var openApiParam = new OpenApiParameter
                        {
                            Name = name,
                            In = fromQuery ? ParameterLocation.Query :
                                fromRoute ? ParameterLocation.Path :
                                ParameterLocation.Query, // default to query
                            Required = fromRoute, // path params are required
                            Schema = context.SchemaGenerator.GenerateSchema(param.ParameterType, context.SchemaRepository)
                        };

                        operation.Parameters.Add(openApiParam);
                    }
                }


                pathItem.Operations[httpMethod] = operation;
            }
        }
    }
}
