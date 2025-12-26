namespace Common.Tests.Utility;

using Common.Exception.Models;
using Common.Utility;

public class AgainstNullOrEmptyTests
{

    [Fact]
    public void AgainstNullOrEmptyWithValidEnumerableDoesNotThrow()
    {
        var items = new List<int> { 1, 2, 3 };

        var exception = Record.Exception(() => Guard.AgainstNullOrEmpty(items, nameof(items)));
        Assert.Null(exception);
    }

    [Fact]
    public void AgainstNullOrEmptyWithNullEnumerableThrowsBadRequestException()
    {
        List<int>? items = null;

        var exception = Assert.Throws<BadRequestException>(() => Guard.AgainstNullOrEmpty(items!, "items"));
        Assert.Contains("items", exception.Message);
        Assert.Contains("null or empty", exception.Message);
    }

    [Fact]
    public void AgainstNullOrEmptyWithEmptyEnumerableThrowsBadRequestException()
    {
        var items = new List<int>();

        var exception = Assert.Throws<BadRequestException>(() => Guard.AgainstNullOrEmpty(items, "items"));
        Assert.Contains("items", exception.Message);
        Assert.Contains("null or empty", exception.Message);
    }
}
