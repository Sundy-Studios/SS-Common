namespace Common.Tests.Exception.Models;

using System.Net;
using Common.Exception.Models;

public class ForbiddenExceptionTests
{
    [Fact]
    public void Default_ConstructsWithForbidden()
    {
        var ex = new ForbiddenException();

        Assert.Equal(HttpStatusCode.Forbidden, ex.StatusCode);
        Assert.NotNull(ex.Message);
    }
}
