namespace Common.Exception.Models;

using System.Net;

public sealed class BadRequestException : CommonHttpException
{
    public BadRequestException()
        : base(HttpStatusCode.BadRequest,
            "The server could not understand the request due to invalid syntax.")
    {
    }

    public BadRequestException(string message)
        : base(HttpStatusCode.BadRequest, message, [message])
    {
    }

    public BadRequestException(IReadOnlyList<string> details)
        : base(HttpStatusCode.BadRequest, null, details)
    {
    }
}
