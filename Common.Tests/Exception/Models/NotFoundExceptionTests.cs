namespace Common.Tests.Exception.Models;

using System.Net;
using Common.Exception.Models;

public class NotFoundExceptionTests
{
    [Fact]
    public void Default_ConstructsWithNotFound()
    {
        var ex = new NotFoundException();

        Assert.Equal(HttpStatusCode.NotFound, ex.StatusCode);
        Assert.NotNull(ex.Message);
    }
}
