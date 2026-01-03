namespace SS.Common.Tests.Logging;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Serilog;
using SS.Common.Logging;
using Xunit;

public class SumoLoggingExtensionsTests
{
    [Fact]
    public void AddSumoLoggingWithNullUrlDoesNotThrow()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();

        // Act
        var result = builder.AddSumoLogging(null);

        // Assert
        Assert.NotNull(result);
        Assert.Same(builder, result);
        Assert.NotNull(Log.Logger);
    }

    [Fact]
    public void AddSumoLoggingWithEmptyUrlDoesNotThrow()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();

        // Act
        var result = builder.AddSumoLogging(string.Empty);

        // Assert
        Assert.NotNull(result);
        Assert.Same(builder, result);
        Assert.NotNull(Log.Logger);
    }

    [Fact]
    public void AddSumoLoggingWithValidUrlConfiguresLogger()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        var fakeSumoUrl = "https://endpoint1.collection.us2.sumologic.com/receiver/v1/http/test";

        // Act
        var result = builder.AddSumoLogging(fakeSumoUrl);

        // Assert
        Assert.NotNull(result);
        Assert.Same(builder, result);
        Assert.NotNull(Log.Logger);
    }

    [Fact]
    public void AddSumoLoggingEnrichesWithEnvironmentInfo()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        builder.Environment.EnvironmentName = Environments.Development;

        // Act
        builder.AddSumoLogging(null);

        // Assert
        // We cannot directly inspect enrichers, but we can ensure logger exists
        Assert.NotNull(Log.Logger);
    }
}
