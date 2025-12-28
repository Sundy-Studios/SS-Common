namespace Common.Tests.Isekai;

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Common.Isekai.Attributes;
using Common.Isekai.Swagger;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

public class IsekaiSwaggerDocumentFilterTests
{
    // Define test interface in this assembly so the filter discovers it
    public interface ISwaggerTestService : Common.Isekai.Services.IIsekaiService
    {
        [IsekaiPath("/test/{id}", IsekaiHttpMethod.Get)]
        [IsekaiResponse(200, typeof(string), "ok")]
        public Task<string> GetById([IsekaiFromRoute] string id);

        [IsekaiPath("/create", IsekaiHttpMethod.Post)]
        public Task Create([IsekaiFromBody] Payload payload);

        [IsekaiPath("/search", IsekaiHttpMethod.Get)]
        public Task Search([IsekaiFromQuery] QueryParams q);
    }

    public class Payload { public string Name { get; set; } = ""; }
    public class QueryParams { public int Page { get; set; } }

    private sealed class FakeSchemaGenerator : ISchemaGenerator
    {
        public static OpenApiSchema GenerateSchema(Type type, SchemaRepository _) => new() { Type = type == typeof(string) ? "string" : "object" };

        public OpenApiSchema GenerateSchema(Type type, SchemaRepository schemaRepository, MemberInfo? memberInfo, ParameterInfo? parameterInfo, ApiParameterRouteInfo? apiParameterRouteInfo) => GenerateSchema(type, schemaRepository);
    }

    [Fact]
    public void ApplyAddsPathsAndOperationsAndParameters()
    {
        var filter = new IsekaiSwaggerDocumentFilter();
        var doc = new OpenApiDocument { Paths = [] };

        var schemaGen = new FakeSchemaGenerator();
        var schemaRepo = new SchemaRepository();

        // Build a DocumentFilterContext via available constructor overload
        var apiDescriptions = new List<ApiDescription>();
        var ctxType = typeof(DocumentFilterContext);
        var ctor = ctxType.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .FirstOrDefault(c => c.GetParameters().Length >= 3);

        var ctx = (DocumentFilterContext)ctor!.Invoke([apiDescriptions, schemaGen, schemaRepo]);

        filter.Apply(doc, ctx);

        // Expect three paths added
        Assert.True(doc.Paths.ContainsKey("/test/{id}"));
        Assert.True(doc.Paths.ContainsKey("/create"));
        Assert.True(doc.Paths.ContainsKey("/search"));

        var getOp = doc.Paths["/test/{id}"].Operations[OperationType.Get];
        Assert.Equal("ok", getOp.Responses["200"].Description);

        // Route param appears as Path parameter
        Assert.Contains(getOp.Parameters, p => p.In == ParameterLocation.Path && p.Name == "id");

        var postOp = doc.Paths["/create"].Operations[OperationType.Post];
        Assert.NotNull(postOp.RequestBody);

        var searchOp = doc.Paths["/search"].Operations[OperationType.Get];
        // query param should be expanded into property parameters (Page)
        Assert.Contains(searchOp.Parameters, p => p.In == ParameterLocation.Query && p.Name == "Page");
    }
}
