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
}
