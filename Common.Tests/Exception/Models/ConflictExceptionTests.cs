namespace Common.Tests.Exception.Models;

using System.Net;
using Common.Exception.Models;

public class ConflictExceptionTests
{
    [Fact]
    public void DefaultConstructsWithConflict()
    {
        var ex = new ConflictException();

        Assert.Equal(HttpStatusCode.Conflict, ex.StatusCode);
        Assert.NotNull(ex.Message);
    }

    [Fact]
    public void StringConstructorSetsMessageAndDetails()
    {
        var ex = new ConflictException("conflict");

        Assert.Equal(HttpStatusCode.Conflict, ex.StatusCode);
        Assert.Equal("conflict", ex.Message);
        Assert.NotNull(ex.Details);
        Assert.Single(ex.Details);
        Assert.Equal("conflict", ex.Details[0]);
    }

    [Fact]
    public void ListConstructorSetsDetails()
    {
        var details = new[] { "d" };
        var ex = new ConflictException(details);

        Assert.Equal(HttpStatusCode.Conflict, ex.StatusCode);
        Assert.NotNull(ex.Details);
        Assert.Equal(1, ex.Details?.Count);
    }
}
