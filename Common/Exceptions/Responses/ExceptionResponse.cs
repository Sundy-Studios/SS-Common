namespace Common.Exceptions.Responses;

public class ExceptionResponse(bool success, int code, string message)
{
    public bool Success { get; set; } = success;
    public int Code { get; set; } = code;
    public string Message { get; set; } = message;
}
