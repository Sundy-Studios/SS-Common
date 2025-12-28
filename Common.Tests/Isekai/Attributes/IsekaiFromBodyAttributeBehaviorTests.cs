namespace Common.Tests.Isekai.Attributes;

using System.Reflection;
using Common.Isekai.Attributes;

public class IsekaiFromBodyAttributeBehaviorTests
{
    private sealed class Dummy
    {
        public static void M([IsekaiFromBody] object _) { }
    }

    [Fact]
    public void ParameterDecoratedWithIsekaiFromBodyIsDiscoverable()
    {
        var method = typeof(Dummy).GetMethod("M")!;
        var param = method.GetParameters()[0];

        var attr = param.GetCustomAttribute<IsekaiFromBodyAttribute>();

        Assert.NotNull(attr);
    }
}
