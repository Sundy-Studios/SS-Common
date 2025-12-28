namespace Common.Tests.Exception.Contracts;

using System.Text.Json;
using Common.Exception.Contracts;

public class ErrorResponseTests
{
    [Fact]
    public void SerializesAndDeserializes()
    {
        var err = new ErrorResponse(false, 400, "Bad", new[] { "d1" });

        var json = JsonSerializer.Serialize(err);
        var round = JsonSerializer.Deserialize<ErrorResponse>(json);

        Assert.NotNull(round);
        Assert.False(round!.Success);
    }
}
