namespace Common.Tests.Isekai.Client;

using System.Net.Http;
using Common.Isekai.Client;
using Common.Isekai.Services;

public class IsekaiClientCreateTests
{
    public interface ITestService : IIsekaiService { }

    [Fact]
    public void CreateReturnsProxyForInterface()
    {
        var http = new HttpClient { BaseAddress = new Uri("http://localhost") };

        var svc = IsekaiClient.Create<ITestService>(http);

        Assert.NotNull(svc);
        Assert.IsType<ITestService>(svc, exactMatch: false);
    }
}
