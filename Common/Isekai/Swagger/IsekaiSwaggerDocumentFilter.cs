namespace Common.Isekai.Swagger;

using System.Reflection;
using Common.Isekai.Attributes;
using Common.Isekai.Services;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

public class IsekaiSwaggerDocumentFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        foreach (var iface in GetIsekaiInterfaces())
        {
            foreach (var method in iface.GetMethods())
            {
                var pathAttr = method.GetCustomAttribute<IsekaiPathAttribute>();
                if (pathAttr == null)
                {
                    continue;
                }

                var path = NormalizePath(pathAttr.Path);
                var httpMethod = MapHttpMethod(pathAttr.Method);

                var operation = CreateBaseOperation(iface, method);

                ApplyInterfaceMetadata(iface, operation, context);
                ApplyMethodMetadata(method, operation, context);
                ApplyParameters(method, operation, context);

                swaggerDoc.Paths.TryAdd(path, new OpenApiPathItem());
                swaggerDoc.Paths[path].Operations[httpMethod] = operation;
            }
        }
    }

    // -------------------------
    // Discovery
    // -------------------------

    private static IEnumerable<Type> GetIsekaiInterfaces() =>
        AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t =>
                t.IsInterface &&
                typeof(IIsekaiService).IsAssignableFrom(t) &&
                t != typeof(IIsekaiService));

    // -------------------------
    // Operation creation
    // -------------------------

    private static OpenApiOperation CreateBaseOperation(Type iface, MethodInfo method) =>
        new()
        {
            Summary = $"Isekai: {iface.Name}.{method.Name}",
            Responses =
            {
                ["200"] = new OpenApiResponse { Description = "Success" }
            }
        };

    // -------------------------
    // Metadata
    // -------------------------

    private static void ApplyInterfaceMetadata(
        Type iface,
        OpenApiOperation operation,
        DocumentFilterContext context)
    {
        ApplySwaggerResponses(
            iface.GetCustomAttributes<IsekaiResponseAttribute>(),
            operation,
            context);

        ApplyConsumes(
            iface.GetCustomAttribute<IsekaiConsumesAttribute>(),
            operation);

        ApplyProduces(
            iface.GetCustomAttribute<IsekaiProducesAttribute>(),
            operation);
    }

    private static void ApplyMethodMetadata(
        MethodInfo method,
        OpenApiOperation operation,
        DocumentFilterContext context)
    {
        if (method.GetCustomAttribute<IsekaiOperationAttribute>() is { } op)
        {
            operation.Summary = op.Summary;
            operation.Description = op.Description;
        }

        ApplySwaggerResponses(
            method.GetCustomAttributes<IsekaiResponseAttribute>(),
            operation,
            context);
    }

    private static void ApplySwaggerResponses(
        IEnumerable<IsekaiResponseAttribute> responses,
        OpenApiOperation operation,
        DocumentFilterContext context)
    {
        foreach (var response in responses)
        {
            operation.Responses[response.StatusCode.ToString()] =
                new OpenApiResponse
                {
                    Description = response.Description,
                    Content =
                    {
                        ["application/json"] = new OpenApiMediaType
                        {
                            Schema = response.ResponseType != null
                                ? context.SchemaGenerator.GenerateSchema(
                                    response.ResponseType,
                                    context.SchemaRepository)
                                : null
                        }
                    }
                };
        }
    }

    private static void ApplyConsumes(
        IsekaiConsumesAttribute? consumes,
        OpenApiOperation operation)
    {
        if (consumes == null)
        {
            return;
        }

        operation.RequestBody ??= new OpenApiRequestBody();

        foreach (var contentType in consumes.ContentTypes)
        {
            operation.RequestBody.Content.TryAdd(
                contentType,
                new OpenApiMediaType());
        }
    }

    private static void ApplyProduces(
        IsekaiProducesAttribute? produces,
        OpenApiOperation operation)
    {
        if (produces == null)
        {
            return;
        }

        foreach (var response in operation.Responses.Values)
        {
            foreach (var contentType in produces.ContentTypes)
            {
                response.Content.TryAdd(
                    contentType,
                    new OpenApiMediaType());
            }
        }
    }

    // -------------------------
    // Parameters
    // -------------------------

    private static void ApplyParameters(
        MethodInfo method,
        OpenApiOperation operation,
        DocumentFilterContext context)
    {
        foreach (var param in method.GetParameters())
        {
            if (param.GetCustomAttribute<IsekaiFromBodyAttribute>() != null)
            {
                ApplyBodyParameter(param, operation, context);
                continue;
            }

            if (param.GetCustomAttribute<IsekaiFromQueryAttribute>() != null)
            {
                ApplyQueryParameter(param, operation, context);
                continue;
            }

            if (param.GetCustomAttribute<IsekaiFromRouteAttribute>() != null)
            {
                ApplyRouteParameter(param, operation, context);
            }
        }
    }

    private static void ApplyBodyParameter(
        ParameterInfo param,
        OpenApiOperation operation,
        DocumentFilterContext context) => operation.RequestBody = new OpenApiRequestBody
        {
            Required = true,
            Content =
            {
                ["application/json"] = new OpenApiMediaType
                {
                    Schema = context.SchemaGenerator.GenerateSchema(
                        param.ParameterType,
                        context.SchemaRepository)
                }
            }
        };

    private static void ApplyQueryParameter(
        ParameterInfo param,
        OpenApiOperation operation,
        DocumentFilterContext context)
    {
        if (param.ParameterType.IsClass && param.ParameterType != typeof(string))
        {
            foreach (var prop in param.ParameterType
                         .GetProperties()
                         .Where(p => p.CanRead && p.CanWrite))
            {
                operation.Parameters.Add(new OpenApiParameter
                {
                    Name = prop.Name,
                    In = ParameterLocation.Query,
                    Required = false,
                    Schema = context.SchemaGenerator.GenerateSchema(
                        prop.PropertyType,
                        context.SchemaRepository)
                });
            }

            return;
        }

        operation.Parameters.Add(new OpenApiParameter
        {
            Name = param.Name!,
            In = ParameterLocation.Query,
            Required = false,
            Schema = context.SchemaGenerator.GenerateSchema(
                param.ParameterType,
                context.SchemaRepository)
        });
    }

    private static void ApplyRouteParameter(
        ParameterInfo param,
        OpenApiOperation operation,
        DocumentFilterContext context) => operation.Parameters.Add(new OpenApiParameter
        {
            Name = param.Name!,
            In = ParameterLocation.Path,
            Required = true,
            Schema = context.SchemaGenerator.GenerateSchema(
                param.ParameterType,
                context.SchemaRepository)
        });

    // -------------------------
    // Utilities
    // -------------------------

    private static string NormalizePath(string path) =>
        path.StartsWith("/") ? path : "/" + path;

    private static OperationType MapHttpMethod(IsekaiHttpMethod method) =>
        method switch
        {
            IsekaiHttpMethod.Get => OperationType.Get,
            IsekaiHttpMethod.Post => OperationType.Post,
            IsekaiHttpMethod.Put => OperationType.Put,
            IsekaiHttpMethod.Delete => OperationType.Delete,
            IsekaiHttpMethod.Patch => OperationType.Patch,
            _ => OperationType.Get
        };
}
