namespace Common.Tests.Isekai.Attributes;

using Common.Isekai.Attributes;

public class IsekaiAllowAnonymousAttributeTests
{
    [Fact]
    public void CanConstruct()
    {
        var attr = new IsekaiAllowAnonymousAttribute();

        Assert.NotNull(attr);
    }
}
