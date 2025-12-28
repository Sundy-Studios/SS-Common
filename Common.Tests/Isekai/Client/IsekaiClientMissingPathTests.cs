namespace Common.Tests.Isekai.Client;

using System.Net.Http;
using System.Threading.Tasks;
using Common.Exception.Models;

public class IsekaiClientMissingPathTests
{
    public interface ITestService : Common.Isekai.Services.IIsekaiService
    {
        Task MissingAttr();
    }

    [Fact]
    public async Task MissingPath_Invocation_ThrowsConflictException()
    {
        var http = new HttpClient { BaseAddress = new System.Uri("http://localhost") };
        var svc = Common.Isekai.Client.IsekaiClient.Create<ITestService>(http);

        await Assert.ThrowsAsync<ConflictException>(() => svc.MissingAttr());
    }
}
