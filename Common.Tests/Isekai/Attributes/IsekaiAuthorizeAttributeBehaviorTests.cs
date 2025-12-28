namespace Common.Tests.Isekai.Attributes;

using Common.Isekai.Attributes;

public class IsekaiAuthorizeAttributeBehaviorTests
{
    [Fact]
    public void DefaultCtorAllowsNullProperties()
    {
        var a = new IsekaiAuthorizeAttribute();

        Assert.Null(a.Policy);
        Assert.Null(a.Roles);
    }

    [Fact]
    public void PolicyCtorSetsPolicy()
    {
        var a = new IsekaiAuthorizeAttribute("PolicyX");

        Assert.Equal("PolicyX", a.Policy);
        Assert.Null(a.Roles);
    }

    [Fact]
    public void PolicyRolesCtorSetsBoth()
    {
        var a = new IsekaiAuthorizeAttribute("P", "R");

        Assert.Equal("P", a.Policy);
        Assert.Equal("R", a.Roles);
    }
}
