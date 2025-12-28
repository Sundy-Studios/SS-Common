namespace Common.Tests.Utility;

using System;
using Common.Utility;

public class AgainstOutOfRangeTests
{
    [Fact]
    public void LessThanMin_ThrowsArgumentOutOfRange()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Guard.AgainstOutOfRange(0, 1, 5, "val"));
    }

    [Fact]
    public void GreaterThanMax_ThrowsArgumentOutOfRange()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Guard.AgainstOutOfRange(10, 1, 5, "val"));
    }

    [Fact]
    public void WithinRange_DoesNotThrow()
    {
        Guard.AgainstOutOfRange(3, 1, 5, "val");
    }

    [Fact]
    public void OnBoundary_DoesNotThrow()
    {
        Guard.AgainstOutOfRange(1, 1, 5, "val");
        Guard.AgainstOutOfRange(5, 1, 5, "val");
    }
}
