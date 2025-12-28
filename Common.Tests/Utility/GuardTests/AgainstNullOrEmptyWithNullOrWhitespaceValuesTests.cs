namespace Common.Tests.Utility;

using Common.Exception.Models;
using Common.Utility;

public class AgainstNullOrEmptyWithNullOrWhitespaceValuesTests
{

    [Fact]
    public void AgainstNullOrEmptyWithNullOrWhitespaceValuesWithValidStringsDoesNotThrow()
    {
        var strings = new List<string> { "value1", "value2", "value3" };

        var exception = Record.Exception(() => Guard.AgainstNullOrEmptyWithNullOrWhitespaceValues(strings, nameof(strings)));
        Assert.Null(exception);
    }

    [Fact]
    public void AgainstNullOrEmptyWithNullOrWhitespaceValuesWithNullCollectionThrowsBadRequestException()
    {
        List<string>? strings = null;

        var exception = Assert.Throws<BadRequestException>(() =>
            Guard.AgainstNullOrEmptyWithNullOrWhitespaceValues(strings!, "strings"));
        Assert.Contains("strings", exception.Message);
        Assert.Contains("null or empty", exception.Message);
    }

    [Fact]
    public void AgainstNullOrEmptyWithNullOrWhitespaceValuesWithEmptyCollectionThrowsBadRequestException()
    {
        var strings = new List<string>();

        var exception = Assert.Throws<BadRequestException>(() =>
            Guard.AgainstNullOrEmptyWithNullOrWhitespaceValues(strings, "strings"));
        Assert.Contains("strings", exception.Message);
        Assert.Contains("null or empty", exception.Message);
    }

    [Fact]
    public void AgainstNullOrEmptyWithNullOrWhitespaceValuesWithNullStringThrowsBadRequestException()
    {
        var strings = new List<string?> { "value1", null, "value2" };

        var exception = Assert.Throws<BadRequestException>(() =>
            Guard.AgainstNullOrEmptyWithNullOrWhitespaceValues(strings!, "strings"));
        Assert.Contains("strings", exception.Message);
        Assert.Contains("null or whitespace", exception.Message);
    }

    [Fact]
    public void AgainstNullOrEmptyWithNullOrWhitespaceValuesWithEmptyStringThrowsBadRequestException()
    {
        var strings = new List<string> { "value1", "", "value2" };

        var exception = Assert.Throws<BadRequestException>(() =>
            Guard.AgainstNullOrEmptyWithNullOrWhitespaceValues(strings, "strings"));
        Assert.Contains("strings", exception.Message);
        Assert.Contains("null or whitespace", exception.Message);
    }

    [Fact]
    public void AgainstNullOrEmptyWithNullOrWhitespaceValuesWithWhitespaceStringThrowsBadRequestException()
    {
        var strings = new List<string> { "value1", "   ", "value2" };

        var exception = Assert.Throws<BadRequestException>(() =>
            Guard.AgainstNullOrEmptyWithNullOrWhitespaceValues(strings, "strings"));
        Assert.Contains("strings", exception.Message);
        Assert.Contains("null or whitespace", exception.Message);
    }

    [Fact]
    public void AgainstNullOrEmptyWithNullOrWhitespaceValuesWithTabStringThrowsBadRequestException()
    {
        var strings = new List<string> { "value1", "\t", "value2" };

        var exception = Assert.Throws<BadRequestException>(() =>
            Guard.AgainstNullOrEmptyWithNullOrWhitespaceValues(strings, "strings"));
        Assert.Contains("strings", exception.Message);
        Assert.Contains("null or whitespace", exception.Message);
    }

    [Fact]
    public void AgainstNullWithComplexObjectReturnsObject()
    {
        var obj = new { Name = "Test", Value = 42 };

        var result = Guard.AgainstNull(obj, nameof(obj));

        Assert.Equal(obj, result);
    }
}
