using Application.Common.Pagination;

namespace API.Extensions;

internal static class HttpExtensions
{
    internal static void AddPaginationHeaders<T>(this HttpResponse response, PagedResult<T> result)
    {
        response.Headers.Append("X-Current-Page", result.PageNumber.ToString());
        response.Headers.Append("X-Page-Size", result.PageSize.ToString());
        response.Headers.Append("X-Total-Items", result.TotalCount.ToString());
        response.Headers.Append("X-Total-Pages", result.TotalPages.ToString());
        
        response.Headers.Append("Access-Control-Expose-Headers", 
            "X-Current-Page,X-Page-Size,X-Total-Items,X-Total-Pages"
        );
    }
}