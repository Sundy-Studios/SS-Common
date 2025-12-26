namespace Common.Exception.Models;

using System.Net;

public sealed class NotFoundException : CommonHttpException
{
    public NotFoundException()
        : base(HttpStatusCode.NotFound,
            "The server can not find the requested resource.")
    {
    }

    public NotFoundException(string message)
        : base(HttpStatusCode.NotFound, message, [message])
    {
    }

    public NotFoundException(IReadOnlyList<string> details)
        : base(HttpStatusCode.NotFound, null, details)
    {
    }
}
