namespace Common.Tests.Auth;
using Common.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public class FirebaseAuthExtensionsTests
{
    [Fact]
    public void AddFirebaseAuthRegistersAuthenticationServices()
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
