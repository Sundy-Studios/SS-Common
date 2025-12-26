namespace Common.Isekai.Attributes;

[AttributeUsage(AttributeTargets.Interface | AttributeTargets.Method, AllowMultiple = false)]
public sealed class IsekaiProducesAttribute(params string[] contentTypes) : Attribute
{
    public string[] ContentTypes { get; } = contentTypes;
}
