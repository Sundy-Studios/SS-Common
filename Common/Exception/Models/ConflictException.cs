namespace Common.Exception.Models;

using System.Net;

public sealed class ConflictException : CommonHttpException
{
    private const string DefaultMessage = "The request could not be completed due to a conflict with the current state of the resource.";

    public ConflictException()
        : base(HttpStatusCode.Conflict, DefaultMessage)
    {
    }

    public ConflictException(string message)
        : base(HttpStatusCode.Conflict, message, [message])
    {
    }
}
