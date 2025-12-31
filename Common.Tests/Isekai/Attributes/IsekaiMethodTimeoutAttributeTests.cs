namespace Common.Tests.Isekai.Attributes;

using Common.Isekai.Attributes;

public class IsekaiMethodTimeoutAttributeTests
{
    [Fact]
    public void ConstructorSetsTimeout()
    {
        var a = new IsekaiMethodTimeoutAttribute(IsekaiMethodTimeout.Medium);

        Assert.Equal(IsekaiMethodTimeout.Medium, a.Timeout);
    }
}
