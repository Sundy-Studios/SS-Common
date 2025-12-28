namespace Common.Tests.Exception.Models;

using System.Net;
using Common.Exception.Models;

public class ConflictExceptionTests
{
    [Fact]
    public void Default_ConstructsWithConflict()
    {
        var ex = new ConflictException();

        Assert.Equal(HttpStatusCode.Conflict, ex.StatusCode);
        Assert.NotNull(ex.Message);
    }
}
