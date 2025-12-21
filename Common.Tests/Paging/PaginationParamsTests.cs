namespace Common.Tests.Paging;

using Common.Paging;

public class PaginationParamsTests
{
    [Fact]
    public void DefaultsAreCorrect()
    {
        var p = new PaginationParams();

        Assert.Equal(1, p.PageNumber);
        Assert.Equal(20, p.PageSize);
        Assert.Equal(0, p.Skip);
    }

    [Fact]
    public void SettingPageSizeAboveMaxIsCapped()
    {
        var p = new PaginationParams { PageSize = 200 };

        Assert.Equal(100, p.PageSize);
    }

    [Fact]
    public void SkipIsCalculatedCorrectly()
    {
        var p = new PaginationParams { PageNumber = 3, PageSize = 15 };

        Assert.Equal(30, p.Skip);
    }
}
