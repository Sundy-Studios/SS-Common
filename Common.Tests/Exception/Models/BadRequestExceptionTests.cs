namespace Common.Tests.Exception.Models;

using System.Net;
using Common.Exception.Models;

public class BadRequestExceptionTests
{
    [Fact]
    public void DefaultConstructsWithBadRequest()
    {
        var ex = new BadRequestException();

        Assert.Equal(HttpStatusCode.BadRequest, ex.StatusCode);
        Assert.NotNull(ex.Message);
    }

    [Fact]
    public void StringConstructorSetsMessageAndDetails()
    {
        var ex = new BadRequestException("oops");

        Assert.Equal(HttpStatusCode.BadRequest, ex.StatusCode);
        Assert.Equal("oops", ex.Message);
        Assert.NotNull(ex.Details);
        Assert.Single(ex.Details);
        Assert.Equal("oops", ex.Details[0]);
    }

    [Fact]
    public void ListConstructorSetsDetails()
    {
        var details = new[] { "a", "b" };
        var ex = new BadRequestException(details);

        Assert.Equal(HttpStatusCode.BadRequest, ex.StatusCode);
        Assert.NotNull(ex.Details);
        Assert.Equal(2, ex.Details?.Count);
        Assert.Equal("a", ex.Details![0]);
    }
}
