namespace Common.Exception.Models;

using System.Net;

public abstract class CommonHttpException(
    HttpStatusCode statusCode,
    string? message = null,
    IReadOnlyList<string>? details = null
    ) : System.Exception(message)
{
    public HttpStatusCode StatusCode { get; } = statusCode;
    public IReadOnlyList<string>? Details { get; } = details;
}
