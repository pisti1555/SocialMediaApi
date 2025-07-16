using System;
using System.Collections.Generic;

namespace Application.Common.Pagination;

public class PagedResult<T> : List<T>
{
    public int PageNumber { get; }
    public int PageSize { get; }
    public int TotalCount { get; }
    public int TotalPages { get; }

    private PagedResult(IEnumerable<T> items, int pageNumber, int pageSize, int count, int totalPages)
    {
        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalCount = count;
        TotalPages = totalPages;
        AddRange(items);
    }

    public static PagedResult<T> Create(IEnumerable<T> items, int count, int pageNumber, int pageSize)
    {
        var totalPages = (int) Math.Ceiling(count / (double) pageSize);
        return new PagedResult<T>(items, pageNumber, pageSize, count, totalPages);
    }
}