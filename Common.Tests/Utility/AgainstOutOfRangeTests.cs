namespace Common.Tests.Utility;

using System;
using Common.Utility;

public class AgainstOutOfRangeTests
{
    [Fact]
    public void LessThanMinThrowsArgumentOutOfRange() => Assert.Throws<ArgumentOutOfRangeException>(() => Guard.AgainstOutOfRange(0, 1, 5, "val"));

    [Fact]
    public void GreaterThanMaxThrowsArgumentOutOfRange() => Assert.Throws<ArgumentOutOfRangeException>(() => Guard.AgainstOutOfRange(10, 1, 5, "val"));

    [Fact]
    public void WithinRangeDoesNotThrow() => Guard.AgainstOutOfRange(3, 1, 5, "val");

    [Fact]
    public void OnBoundaryDoesNotThrow()
    {
        Guard.AgainstOutOfRange(1, 1, 5, "val");
        Guard.AgainstOutOfRange(5, 1, 5, "val");
    }
}
