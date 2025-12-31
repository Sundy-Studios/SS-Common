namespace Common.Tests.Isekai.Attributes;

using Common.Isekai.Attributes;

public class IsekaiProducesAttributeTests
{
    [Fact]
    public void ConstructorSetsContentTypes()
    {
        var a = new IsekaiProducesAttribute("application/json", "text/plain");

        Assert.Equal(2, a.ContentTypes.Length);
        Assert.Contains("application/json", a.ContentTypes);
    }
}
