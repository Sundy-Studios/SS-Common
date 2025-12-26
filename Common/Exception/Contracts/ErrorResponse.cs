namespace Common.Exception.Contracts;

public sealed record ErrorResponse(
    bool Success,
    int StatusCode,
    string Message,
    IReadOnlyList<string>? Details = null
);
