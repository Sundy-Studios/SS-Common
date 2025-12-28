namespace Common.Tests.Isekai.Attributes;

using Common.Isekai.Attributes;

public class IsekaiResponseAttributeTests
{
    [Fact]
    public void ConstructorSetsValues()
    {
        var a = new IsekaiResponseAttribute(201, typeof(string), "desc");

        Assert.Equal(201, a.StatusCode);
        Assert.Equal(typeof(string), a.ResponseType);
        Assert.Equal("desc", a.Description);
    }

    [Fact]
    public void ConstructorAllowsNulls()
    {
        var a = new IsekaiResponseAttribute(404);

        Assert.Equal(404, a.StatusCode);
        Assert.Null(a.ResponseType);
        Assert.Null(a.Description);
    }
}
