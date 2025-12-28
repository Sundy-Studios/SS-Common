namespace Common.Tests.Isekai.Attributes;

using System.Reflection;
using Common.Isekai.Attributes;

public class IsekaiFromRouteAttributeBehaviorTests
{
    private sealed class Dummy
    {
        public static void M([IsekaiFromRoute] string _) { }
    }

    [Fact]
    public void ParameterDecoratedWithIsekaiFromRouteIsDiscoverable()
    {
        var method = typeof(Dummy).GetMethod("M")!;
        var param = method.GetParameters()[0];

        var attr = param.GetCustomAttribute<IsekaiFromRouteAttribute>();

        Assert.NotNull(attr);
    }
}
