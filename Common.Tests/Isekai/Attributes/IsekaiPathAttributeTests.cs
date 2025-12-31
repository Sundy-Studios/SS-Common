namespace Common.Tests.Isekai.Attributes;

using Common.Isekai.Attributes;

public class IsekaiPathAttributeTests
{
    [Fact]
    public void ConstructorSetsPathAndDefaultMethod()
    {
        var a = new IsekaiPathAttribute("/x");

        Assert.Equal("/x", a.Path);
        Assert.Equal(IsekaiHttpMethod.Get, a.Method);
    }

    [Fact]
    public void ConstructorSetsMethod()
    {
        var a = new IsekaiPathAttribute("/y", IsekaiHttpMethod.Post);

        Assert.Equal("/y", a.Path);
        Assert.Equal(IsekaiHttpMethod.Post, a.Method);
    }
}
