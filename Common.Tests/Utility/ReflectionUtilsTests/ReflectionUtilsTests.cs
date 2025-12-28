namespace Common.Tests.Utility;

using System.Reflection;
using Common.Utility;

public class ReflectionUtilsTests
{
    [Fact]
    public void GetLoadableTypes_ReturnsTypesFromAssembly()
    {
        var asm = typeof(ReflectionUtils).Assembly;

        var types = ReflectionUtils.GetLoadableTypes(asm).ToArray();

        Assert.Contains(typeof(ReflectionUtils), types);
    }
}
