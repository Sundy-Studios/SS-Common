namespace Common.Exception.Models;

using System.Net;

public sealed class UnauthorizedException : CommonHttpException
{
    public UnauthorizedException()
        : base(HttpStatusCode.Unauthorized,
            "Network credentials are no longer valid.")
    {
    }

    public UnauthorizedException(string message)
        : base(HttpStatusCode.Unauthorized, message, [message])
    {
    }

    public UnauthorizedException(IReadOnlyList<string> details)
        : base(HttpStatusCode.Unauthorized, null, details)
    {
    }
}
