namespace Common.Isekai.Attributes;

[AttributeUsage(AttributeTargets.Interface | AttributeTargets.Method, AllowMultiple = true)]
public sealed class IsekaiResponseAttribute(int statusCode, Type? responseType = null, string? description = null) : Attribute
{
    public int StatusCode { get; } = statusCode;
    public Type? ResponseType { get; } = responseType;
    public string? Description { get; } = description;
}
