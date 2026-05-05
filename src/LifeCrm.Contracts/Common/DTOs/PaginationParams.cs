namespace LifeCrm.Contracts.Common.DTOs;
public record PaginationParams
{
    private const int MaxPageSize = 100;
    private int _pageSize = 25;
    public int Page { get; init; } = 1;
    public int PageSize { get => _pageSize; init => _pageSize = value > MaxPageSize ? MaxPageSize : value < 1 ? 1 : value; }
    public string? Search { get; init; }
    public string? SortBy { get; init; }
    public bool SortAscending { get; init; } = true;
}
