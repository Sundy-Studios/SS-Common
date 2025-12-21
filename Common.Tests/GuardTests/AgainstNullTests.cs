namespace Common.Tests.Utility;
using Common.Utility;

public class AgainstNullTests
{
    [Fact]
    public void AgainstNullWithNonNullValueReturnsValue()
    {
        var value = "test";

        var result = Guard.AgainstNull(value, nameof(value));

        Assert.Equal(value, result);
    }

    [Fact]
    public void AgainstNullWithNullValueThrowsArgumentNullException()
    {
        string? value = null;

        var exception = Assert.Throws<ArgumentNullException>(() => Guard.AgainstNull(value!, "testParam"));
        Assert.Contains("testParam", exception.Message);
    }

    [Fact]
    public void AgainstNullWithNullValueAndNoNameThrowsWithEmptyName()
    {
        string? value = null;

        Assert.Throws<ArgumentNullException>(() => Guard.AgainstNull(value!));
    }
}
