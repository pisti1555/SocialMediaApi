namespace API.DTOs.Params;

public record PaginationParams
{
    public int PageNumber { get; init; } = 0;
    public int PageSize { get; init; } = 0;
}