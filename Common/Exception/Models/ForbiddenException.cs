namespace Common.Exception.Models;

using System.Net;

public sealed class ForbiddenException : CommonHttpException
{
    public ForbiddenException()
        : base(HttpStatusCode.Forbidden,
            "The client does not have access rights to the content.")
    {
    }

    public ForbiddenException(string message)
        : base(HttpStatusCode.Forbidden, message, [message])
    {
    }

    public ForbiddenException(IReadOnlyList<string> details)
        : base(HttpStatusCode.Forbidden, null, details)
    {
    }
}
