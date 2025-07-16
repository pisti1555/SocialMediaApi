namespace Application.Common.Pagination;

public record PaginationAttributes
{
    private const int MaxPageSize = 40;

    private int _pageNumber = 1;
    private int _pageSize = 10;

    public int PageNumber
    {
        get => _pageNumber;
        set => _pageNumber = (value >= 1) ? value : 1;
    }

    public int PageSize
    {
        get => _pageSize;
        set
        {
            _pageSize = value switch
            {
                <= 0 => 10,
                >= MaxPageSize => MaxPageSize,
                _ => _pageSize
            };
        }
    }
}