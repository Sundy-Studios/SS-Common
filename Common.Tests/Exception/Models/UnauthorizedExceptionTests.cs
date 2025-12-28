namespace Common.Tests.Exception.Models;

using System.Net;
using Common.Exception.Models;

public class UnauthorizedExceptionTests
{
    [Fact]
    public void Default_ConstructsWithUnauthorized()
    {
        var ex = new UnauthorizedException();

        Assert.Equal(HttpStatusCode.Unauthorized, ex.StatusCode);
        Assert.NotNull(ex.Message);
    }
}
