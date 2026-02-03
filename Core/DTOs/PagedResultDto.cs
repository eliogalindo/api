namespace api.Core.DTOs;

public class PagedResultDto<T>
{
    public List<T> Items { get; set; } = [];
    public int Count { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    private int TotalPages => (int)Math.Ceiling((double)Count / PageSize);
    public bool HasNextPage => PageNumber < TotalPages;
    public bool HasPreviousPage => PageNumber > 1;
}