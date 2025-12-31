namespace Common.Tests.Isekai;

using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Common.Isekai.Attributes;
using Common.Isekai.Client;
using Common.Isekai.Services;

public class IsekaiClientInvocationTests
{
    public interface ITestApi : IIsekaiService
    {
        [IsekaiPath("/item/{id}", IsekaiHttpMethod.Get)]
        public Task<string> GetItem(string id);
    }

    private sealed class FakeHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create("hello")
            };
            return Task.FromResult(response);
        }
    }

    [Fact]
    public async Task ProxyInvokeCallsHttpClientAndReturnsValue()
    {
        var http = new HttpClient(new FakeHandler()) { BaseAddress = new Uri("http://localhost") };

        var api = IsekaiClient.Create<ITestApi>(http);

        var result = await api.GetItem("1");

        Assert.Equal("hello", result);
    }
}
