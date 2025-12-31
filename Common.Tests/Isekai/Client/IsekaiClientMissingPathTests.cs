namespace Common.Tests.Isekai.Client;

using System.Net.Http;
using System.Threading.Tasks;
using Common.Exception.Models;

public class IsekaiClientMissingPathTests
{
    public interface ITestService : Common.Isekai.Services.IIsekaiService
    {
        public Task MissingAttr();
    }

    [Fact]
    public async Task MissingPathInvocationThrowsConflictException()
    {
        var http = new HttpClient { BaseAddress = new Uri("http://localhost") };
        var svc = Common.Isekai.Client.IsekaiClient.Create<ITestService>(http);

        await Assert.ThrowsAsync<ConflictException>(svc.MissingAttr);
    }
}
