namespace Common.Tests.Isekai.Attributes;

using Common.Isekai.Attributes;

public class IsekaiOperationAttributeTests
{
    [Fact]
    public void PropertiesCanBeSetAndRead()
    {
        var a = new IsekaiOperationAttribute { Summary = "s", Description = "d" };

        Assert.Equal("s", a.Summary);
        Assert.Equal("d", a.Description);
    }
}
