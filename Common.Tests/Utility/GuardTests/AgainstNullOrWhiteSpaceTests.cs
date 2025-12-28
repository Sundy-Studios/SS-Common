namespace Common.Tests.Utility;

using Common.Exception.Models;
using Common.Utility;

public class AgainstNullOrWhiteSpaceTests
{
    [Fact]
    public void NullThrowsBadRequestException() => Assert.Throws<BadRequestException>(() => Guard.AgainstNullOrWhiteSpace(null, "param"));

    [Fact]
    public void EmptyThrowsBadRequestException() => Assert.Throws<BadRequestException>(() => Guard.AgainstNullOrWhiteSpace(string.Empty, "param"));

    [Fact]
    public void WhiteSpaceThrowsBadRequestException() => Assert.Throws<BadRequestException>(() => Guard.AgainstNullOrWhiteSpace("   ", "param"));

    [Fact]
    public void ValidDoesNotThrow() => Guard.AgainstNullOrWhiteSpace("value", "param");
}
