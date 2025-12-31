namespace Common.Tests.Isekai.Attributes;

using Common.Isekai.Attributes;

public class IsekaiGateAttributeTests
{
    [Fact]
    public void ConstructorSetsClientAndServiceNames()
    {
        var a = new IsekaiGateAttribute("client", "service");

        Assert.Equal("client", a.ClientName);
        Assert.Equal("service", a.ServiceName);
    }
}
