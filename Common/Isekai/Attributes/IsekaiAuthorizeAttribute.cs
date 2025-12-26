namespace Common.Isekai.Attributes;

[AttributeUsage(AttributeTargets.Interface | AttributeTargets.Method, AllowMultiple = true)]
public sealed class IsekaiAuthorizeAttribute : Attribute
{
    public string? Policy { get; }
    public string? Roles { get; }

    public IsekaiAuthorizeAttribute()
    {
    }

    public IsekaiAuthorizeAttribute(string policy) => Policy = policy;

    public IsekaiAuthorizeAttribute(string policy, string roles)
    {
        Policy = policy;
        Roles = roles;
    }
}
