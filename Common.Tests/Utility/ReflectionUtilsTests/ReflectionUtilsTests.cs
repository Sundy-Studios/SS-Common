namespace Common.Tests.Utility;

using Common.Utility;

public class ReflectionUtilsTests
{
    [Fact]
    public void GetLoadableTypesReturnsTypesFromAssembly()
    {
        var asm = typeof(ReflectionUtils).Assembly;

        var types = ReflectionUtils.GetLoadableTypes(asm).ToArray();

        Assert.Contains(typeof(ReflectionUtils), types);
    }
}
