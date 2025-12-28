namespace Common.Tests.Utility;

using Common.Exception.Models;
using Common.Utility;

public class AgainstNullOrWhiteSpaceTests
{
    [Fact]
    public void Null_ThrowsBadRequestException()
    {
        Assert.Throws<BadRequestException>(() => Guard.AgainstNullOrWhiteSpace(null, "param"));
    }

    [Fact]
    public void Empty_ThrowsBadRequestException()
    {
        Assert.Throws<BadRequestException>(() => Guard.AgainstNullOrWhiteSpace(string.Empty, "param"));
    }

    [Fact]
    public void WhiteSpace_ThrowsBadRequestException()
    {
        Assert.Throws<BadRequestException>(() => Guard.AgainstNullOrWhiteSpace("   ", "param"));
    }

    [Fact]
    public void Valid_DoesNotThrow()
    {
        Guard.AgainstNullOrWhiteSpace("value", "param");
    }
}
