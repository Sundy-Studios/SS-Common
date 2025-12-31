namespace Common.Tests.Exception.Models;

using System.Net;
using Common.Exception.Models;

public class UnauthorizedExceptionTests
{
    [Fact]
    public void DefaultConstructsWithUnauthorized()
    {
        var ex = new UnauthorizedException();

        Assert.Equal(HttpStatusCode.Unauthorized, ex.StatusCode);
        Assert.NotNull(ex.Message);
    }

    [Fact]
    public void StringConstructorSetsMessageAndDetails()
    {
        var ex = new UnauthorizedException("nope");

        Assert.Equal(HttpStatusCode.Unauthorized, ex.StatusCode);
        Assert.Equal("nope", ex.Message);
        Assert.NotNull(ex.Details);
        Assert.Single(ex.Details);
        Assert.Equal("nope", ex.Details[0]);
    }

    [Fact]
    public void ListConstructorSetsDetails()
    {
        var details = new[] { "d1", "d2" };
        var ex = new UnauthorizedException(details);

        Assert.Equal(HttpStatusCode.Unauthorized, ex.StatusCode);
        Assert.NotNull(ex.Details);
        Assert.Equal(2, ex.Details?.Count);
    }
}
