namespace Common.Attributes.Isekai;

[AttributeUsage(AttributeTargets.Method)]
public sealed class IsekaiMethodTimeoutAttribute(IsekaiMethodTimeout timeout) : Attribute
{
    public IsekaiMethodTimeout Timeout { get; private set; } = timeout;
}
