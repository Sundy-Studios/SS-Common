namespace Common.Paging;

public class PaginationParams
{
    private const int MaxPageSize = 100;

    public int PageNumber { get; set; } = 1;
    public int PageSize
    {
        get;
        set => field = value > MaxPageSize ? MaxPageSize : value;
    } = 20;

    public int Skip => (PageNumber - 1) * PageSize;
}
