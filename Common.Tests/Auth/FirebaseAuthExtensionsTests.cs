using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Common.Tests.Auth;

public class FirebaseAuthExtensionsTests
{
    [Fact]
    public void AddFirebaseAuth_RegistersAuthenticationServices()
    {
        var services = new ServiceCollection();
        var inMemory = new Dictionary<string, string?> { ["Firebase:ProjectId"] = "test-proj" };
        var config = new ConfigurationBuilder().AddInMemoryCollection(inMemory).Build();

        services.AddFirebaseAuth(config);

        var provider = services.BuildServiceProvider();

        var authService = provider.GetService(typeof(Microsoft.AspNetCore.Authentication.IAuthenticationService));

        Assert.NotNull(authService);
    }
}
