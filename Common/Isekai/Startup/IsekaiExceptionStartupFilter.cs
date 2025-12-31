namespace Common.Isekai.Startup;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Exception.Contracts;
using Common.Exception.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

internal sealed class IsekaiExceptionStartupFilter : IStartupFilter
{
    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) => app =>
    {
        app.Use(async (ctx, nextMiddleware) =>
        {
            Exception? exception = null;

            try
            {
                await nextMiddleware();
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            if (ctx.Response.HasStarted)
            {
                return;
            }

            var statusCode = exception switch
            {
                UnauthorizedException => StatusCodes.Status401Unauthorized,
                ForbiddenException => StatusCodes.Status403Forbidden,
                NotFoundException => StatusCodes.Status404NotFound,
                _ when exception != null => StatusCodes.Status500InternalServerError,
                _ => ctx.Response.StatusCode
            };

            if (statusCode < StatusCodes.Status400BadRequest)
            {
                return;
            }

            var message = ResolveMessage(statusCode, exception);

            await WriteError(ctx, statusCode, message, exception);
        });

        next(app);
    };

    private static string ResolveMessage(int statusCode, Exception? ex)
    {
        if (!string.IsNullOrWhiteSpace(ex?.Message))
        {
            return ex.Message;
        }

        return statusCode switch
        {
            StatusCodes.Status400BadRequest => "Bad request",
            StatusCodes.Status401Unauthorized => "Unauthorized",
            StatusCodes.Status403Forbidden => "Forbidden",
            StatusCodes.Status404NotFound => "Not found",
            StatusCodes.Status405MethodNotAllowed => "Method not allowed",
            StatusCodes.Status409Conflict => "Conflict",
            StatusCodes.Status422UnprocessableEntity => "Unprocessable entity",
            _ => "An unexpected error occurred"
        };
    }

    private static Task WriteError(
        HttpContext ctx,
        int statusCode,
        string message,
        Exception? ex)
    {
        ctx.Response.Clear();
        ctx.Response.StatusCode = statusCode;
        ctx.Response.ContentType = "application/json";

        List<string>? details = null;

#if DEBUG
        if (ex?.StackTrace != null)
        {
            details = [ex.StackTrace];
        }
#endif

        var error = new ErrorResponse(
            Success: false,
            StatusCode: statusCode,
            Message: message,
            Details: details);

        return ctx.Response.WriteAsJsonAsync(error);
    }
}
