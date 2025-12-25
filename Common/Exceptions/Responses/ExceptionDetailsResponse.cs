namespace Common.Exceptions.Responses;

public class ExceptionDetailsResponse(bool success, int code, string message, List<string> details)
{
    public bool Success { get; set; } = success;
    public int Code { get; set; } = code;
    public string Message { get; set; } = message;
    public List<string> Details { get; set; } = details;
}
