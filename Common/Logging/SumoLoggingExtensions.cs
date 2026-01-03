namespace SS.Common.Logging;

using System.Globalization;
using Microsoft.AspNetCore.Builder;
using Serilog;
using Serilog.Sinks.SumoLogic;

public static class SumoLoggingExtensions
{
    public static WebApplicationBuilder AddSumoLogging(
        this WebApplicationBuilder builder,
        string? sumoUrl)
    {
        var loggerConfig = new LoggerConfiguration()
            .MinimumLevel.Information()
            .Enrich.FromLogContext()
            .Enrich.WithProperty("org", "SS")
            .Enrich.WithProperty("service", builder.Environment.ApplicationName)
            .Enrich.WithProperty("env", builder.Environment.EnvironmentName);

        if (!string.IsNullOrWhiteSpace(sumoUrl))
        {
            loggerConfig = loggerConfig
                .WriteTo.SumoLogic(sumoUrl);
        }

        // Always log to console
        loggerConfig = loggerConfig.WriteTo.Console(
            formatProvider: CultureInfo.InvariantCulture
        );

        Log.Logger = loggerConfig.CreateLogger();

        builder.Host.UseSerilog();

        return builder;
    }
}
