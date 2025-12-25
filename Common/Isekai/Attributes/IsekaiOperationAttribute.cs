namespace Common.Isekai.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public sealed class IsekaiOperationAttribute : Attribute
{
    public string? Summary { get; set; }
    public string? Description { get; set; }
}
