namespace Common.Tests.Isekai.Attributes;

using Common.Isekai.Attributes;

public class IsekaiConsumesAttributeTests
{
    [Fact]
    public void ConstructorSetsContentTypes()
    {
        var a = new IsekaiConsumesAttribute("application/json");

        Assert.Single(a.ContentTypes);
        Assert.Equal("application/json", a.ContentTypes[0]);
    }
}
