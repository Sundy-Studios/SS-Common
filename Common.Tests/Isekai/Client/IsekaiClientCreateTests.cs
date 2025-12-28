namespace Common.Tests.Isekai.Client;

using System.Net.Http;
using Common.Isekai.Client;
using Common.Isekai.Services;

public class IsekaiClientCreateTests
{
    public interface ITestService : IIsekaiService { }

    [Fact]
    public void Create_ReturnsProxyForInterface()
    {
        var http = new HttpClient { BaseAddress = new System.Uri("http://localhost") };

        var svc = IsekaiClient.Create<ITestService>(http);

        Assert.NotNull(svc);
        Assert.IsAssignableFrom<ITestService>(svc);
    }
}
