namespace Common.Tests.Isekai.Attributes;

using System.Reflection;
using Common.Isekai.Attributes;

public class IsekaiFromQueryAttributeBehaviorTests
{
    private sealed class Dummy
    {
        public static void M([IsekaiFromQuery] string _) { }
    }

    [Fact]
    public void ParameterDecoratedWithIsekaiFromQueryIsDiscoverable()
    {
        var method = typeof(Dummy).GetMethod("M")!;
        var param = method.GetParameters()[0];

        var attr = param.GetCustomAttribute<IsekaiFromQueryAttribute>();

        Assert.NotNull(attr);
    }
}
