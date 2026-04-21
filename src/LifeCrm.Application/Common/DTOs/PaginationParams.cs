namespace LifeCrm.Application.Common.DTOs;

public class PaginationParams
{
    public int    Page          { get; set; } = 1;
    public int    PageSize      { get; set; } = 25;
    public string? Search       { get; set; }
    public string? SortBy       { get; set; }
    public bool   SortAscending { get; set; } = true;
}
