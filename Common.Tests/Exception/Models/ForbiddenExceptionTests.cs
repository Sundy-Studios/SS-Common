namespace Common.Tests.Exception.Models;

using System.Net;
using Common.Exception.Models;

public class ForbiddenExceptionTests
{
    [Fact]
    public void DefaultConstructsWithForbidden()
    {
        var ex = new ForbiddenException();

        Assert.Equal(HttpStatusCode.Forbidden, ex.StatusCode);
        Assert.NotNull(ex.Message);
    }

    [Fact]
    public void StringConstructorSetsMessageAndDetails()
    {
        var ex = new ForbiddenException("no access");

        Assert.Equal(HttpStatusCode.Forbidden, ex.StatusCode);
        Assert.Equal("no access", ex.Message);
        Assert.NotNull(ex.Details);
        Assert.Single(ex.Details);
        Assert.Equal("no access", ex.Details[0]);
    }

    [Fact]
    public void ListConstructorSetsDetails()
    {
        var details = new[] { "reason1" };
        var ex = new ForbiddenException(details);

        Assert.Equal(HttpStatusCode.Forbidden, ex.StatusCode);
        Assert.NotNull(ex.Details);
        Assert.Equal(1, ex.Details?.Count);
    }
}
