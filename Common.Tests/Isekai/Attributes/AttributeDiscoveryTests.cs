namespace Common.Tests.Isekai.Attributes;

using System;
using System.Linq;
using System.Reflection;
using Common.Isekai.Attributes;

public class AttributeDiscoveryTests
{
    [Fact]
    public void AllAttributesAreAttributesAndHaveUsage()
    {
        var asm = typeof(IsekaiAuthorizeAttribute).Assembly;
        var attrTypes = asm.GetTypes()
            .Where(t => t.IsClass && t.Namespace == "Common.Isekai.Attributes");

        foreach (var t in attrTypes)
        {
            Assert.True(typeof(Attribute).IsAssignableFrom(t), $"{t.FullName} must inherit Attribute");

            var usage = t.GetCustomAttribute<AttributeUsageAttribute>();
            Assert.NotNull(usage);

            var ctor = t.GetConstructor(Type.EmptyTypes);
            if (ctor != null)
            {
                var inst = Activator.CreateInstance(t);
                Assert.NotNull(inst);

                foreach (var p in t.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    _ = p.GetValue(inst);
                }
            }
        }
    }
}
