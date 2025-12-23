namespace Common.Attributes.Isekai;

public enum IsekaiHttpMethod
{
    Get,
    Post,
    Put,
    Delete,
    Patch
}

[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public sealed class IsekaiPathAttribute(string path, IsekaiHttpMethod method = IsekaiHttpMethod.Get) : Attribute
{
    public string Path { get; } = path;
    public IsekaiHttpMethod Method { get; } = method;
}
