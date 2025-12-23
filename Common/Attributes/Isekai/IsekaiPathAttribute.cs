namespace Common.Attributes.Isekai;

[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public sealed class IsekaiPathAttribute(string path) : Attribute
{
    public string Path { get; } = path;
}
