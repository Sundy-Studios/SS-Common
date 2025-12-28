namespace Common.Tests.Isekai;

using System.Reflection;
using Common.Isekai.Attributes;
using Common.Isekai.Services;
using Common.Isekai.Startup;
using Microsoft.AspNetCore.Builder;

public class IsekaiEndpointMapperTests
{
    public interface IMapTestService : IIsekaiService
    {
        [IsekaiPath("/map/test")]
        Task DoThing();
    }

    public class MapTestService : IMapTestService
    {
        public Task DoThing() => Task.CompletedTask;
    }

    [Fact]
    public void MapIsekaiEndpoints_DoesNotThrow()
    {
        var builder = WebApplication.CreateBuilder();
        var app = builder.Build();

        // Should not throw when mapping endpoints from this test assembly
        var res = app.MapIsekaiEndpoints(Assembly.GetExecutingAssembly());

        Assert.NotNull(res);
    }
}
