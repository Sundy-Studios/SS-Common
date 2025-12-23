namespace Common.Attributes.Isekai;

[AttributeUsage(AttributeTargets.Interface, Inherited = false)]
public sealed class IsekaiGateAttribute(string clientName, string serviceName) : Attribute
{
    public string ClientName { get; } = clientName;
    public string ServiceName { get; } = serviceName;
}
