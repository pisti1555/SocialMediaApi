namespace Application.Common.Pagination;

public class PagedResult<T>
{
    public List<T> Items { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    
    public PagedResult() { }

    private PagedResult(IEnumerable<T> items, int pageNumber, int pageSize, int count, int totalPages)
    {
        Items = items.ToList();
        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalCount = count;
        TotalPages = totalPages;
    }

    public static PagedResult<T> Create(IEnumerable<T> items, int count, int pageNumber, int pageSize)
    {
        var totalPages = (int) Math.Ceiling(count / (double) pageSize);
        return new PagedResult<T>(items, pageNumber, pageSize, count, totalPages);
    }
}