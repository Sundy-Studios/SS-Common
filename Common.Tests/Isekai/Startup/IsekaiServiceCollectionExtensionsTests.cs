namespace Common.Tests.Isekai;

using System;
using System.Net.Http;
using Common.Isekai.Services;
using Common.Isekai.Startup;
using Microsoft.Extensions.DependencyInjection;

public class IsekaiServiceCollectionExtensionsTests
{
    public interface ITestSvc : IIsekaiService { }

    public class TestSvc : ITestSvc { }

    [Fact]
    public void AddIsekaiRegistersServiceImplementation()
    {
        var services = new ServiceCollection();

        // Create a dynamic assembly containing an interface that extends IIsekaiService and a concrete implementation
        var asm = CreateDynamicAssemblyWithService();

        services.AddIsekai(asm);

        var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();

        // Resolve the dynamically created service by looking up the interface type from the assembly
        var iface = asm.GetTypes().First(t => t.IsInterface && typeof(IIsekaiService).IsAssignableFrom(t));
        var resolved = scope.ServiceProvider.GetService(iface);
        Assert.NotNull(resolved);
    }

    private static System.Reflection.Assembly CreateDynamicAssemblyWithService()
    {
        var asmName = new System.Reflection.AssemblyName("DynamicIsekaiTestAssembly");
        var asmBuilder = System.Reflection.Emit.AssemblyBuilder.DefineDynamicAssembly(asmName, System.Reflection.Emit.AssemblyBuilderAccess.Run);
        var mod = asmBuilder.DefineDynamicModule("MainModule");

        // Define interface that extends IIsekaiService
        var ifaceBuilder = mod.DefineType("DynamicNamespace.IDynamicService", System.Reflection.TypeAttributes.Interface | System.Reflection.TypeAttributes.Public | System.Reflection.TypeAttributes.Abstract);
        ifaceBuilder.AddInterfaceImplementation(typeof(IIsekaiService));
        var ifaceType = ifaceBuilder.CreateType()!;

        // Define class that implements the interface
        var typeBuilder = mod.DefineType("DynamicNamespace.DynamicService", System.Reflection.TypeAttributes.Public | System.Reflection.TypeAttributes.Class);
        typeBuilder.AddInterfaceImplementation(ifaceType);
        // Add parameterless constructor
        var ctor = typeBuilder.DefineDefaultConstructor(System.Reflection.MethodAttributes.Public);
        var implType = typeBuilder.CreateType()!;

        return asmBuilder;
    }

    [Fact]
    public void AddIsekaiClientRegistersClientSingleton()
    {
        var services = new ServiceCollection();
        var http = new HttpClient { BaseAddress = new Uri("http://localhost") };

        services.AddIsekaiClient<ITestSvc>(http);

        var provider = services.BuildServiceProvider();

        var client = provider.GetService<ITestSvc>();
        Assert.NotNull(client);
    }
}
