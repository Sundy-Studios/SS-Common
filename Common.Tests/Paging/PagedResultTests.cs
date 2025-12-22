namespace Common.Tests.Paging;

using Common.Paging;

public class PagedResultTests
{
    [Fact]
    public void CreateSetsPropertiesAndCalculatesTotalPages()
    {
        var items = new List<int> { 1, 2, 3 };

        var result = PagedResultFactory.Create(items, pageNumber: 2, pageSize: 10, totalItems: 25);

        Assert.Equal(items, result.Items);
        Assert.Equal(2, result.PageNumber);
        Assert.Equal(10, result.PageSize);
        Assert.Equal(25, result.TotalItems);
        Assert.Equal(3, result.TotalPages);
    }

    [Fact]
    public void CreateWithZeroTotalItemsResultsZeroTotalPages()
    {
        var items = Array.Empty<int>();

        var result = PagedResultFactory.Create(items, pageNumber: 1, pageSize: 10, totalItems: 0);

        Assert.Equal(0, result.TotalPages);
    }
}
