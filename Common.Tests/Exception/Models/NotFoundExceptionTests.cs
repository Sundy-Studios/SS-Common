namespace Common.Tests.Exception.Models;

using System.Net;
using Common.Exception.Models;

public class NotFoundExceptionTests
{
    [Fact]
    public void DefaultConstructsWithNotFound()
    {
        var ex = new NotFoundException();

        Assert.Equal(HttpStatusCode.NotFound, ex.StatusCode);
        Assert.NotNull(ex.Message);
    }

    [Fact]
    public void StringConstructorSetsMessageAndDetails()
    {
        var ex = new NotFoundException("missing");

        Assert.Equal(HttpStatusCode.NotFound, ex.StatusCode);
        Assert.Equal("missing", ex.Message);
        Assert.NotNull(ex.Details);
        Assert.Single(ex.Details);
        Assert.Equal("missing", ex.Details[0]);
    }

    [Fact]
    public void ListConstructorSetsDetails()
    {
        var details = new[] { "x" };
        var ex = new NotFoundException(details);

        Assert.Equal(HttpStatusCode.NotFound, ex.StatusCode);
        Assert.NotNull(ex.Details);
        Assert.Equal(1, ex.Details?.Count);
        Assert.Equal("x", ex.Details![0]);
    }
}
