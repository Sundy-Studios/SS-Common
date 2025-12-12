using Common.Auth;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Moq;

namespace Common.Tests.Auth;

public class FirebaseMiddlewareExtensionsTests
{
    [Fact]
    public void UseFirebaseAuth_InvokesUseAtLeastTwice()
    {
        var mock = new Mock<IApplicationBuilder>();
        mock.Setup(m => m.Use(It.IsAny<Func<RequestDelegate, RequestDelegate>>()))
            .Returns(mock.Object);

        var app = mock.Object;

        app.UseFirebaseAuth();

        mock.Verify(m => m.Use(It.IsAny<Func<RequestDelegate, RequestDelegate>>()), Times.AtLeast(2));
    }
}
