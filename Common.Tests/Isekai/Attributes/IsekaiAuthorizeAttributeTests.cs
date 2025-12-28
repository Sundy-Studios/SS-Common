namespace Common.Tests.Isekai.Attributes;

using Common.Isekai.Attributes;

public class IsekaiAuthorizeAttributeTests
{
    [Fact]
    public void PolicyAndRoles_AreSetFromCtor()
    {
        var attr = new IsekaiAuthorizeAttribute("PolicyX", "RoleA");

        Assert.Equal("PolicyX", attr.Policy);
        Assert.Equal("RoleA", attr.Roles);
    }
}
