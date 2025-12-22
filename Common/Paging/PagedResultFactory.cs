namespace Common.Paging;

public static class PagedResultFactory
{
    public static PagedResult<T> Create<T>(
        IReadOnlyList<T> items,
        int pageNumber,
        int pageSize,
        long totalItems)
    {
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

        return new PagedResult<T>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = totalPages
        };
    }
}
