namespace api.Core.DTOs;

public class SearchParamsDto
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10000;
    public string? OrderBy { get; set; }
    public bool Desc { get; set; } = false;
    public string? Filter { get; set; }
    public bool? AllSelected { get; set; } = false;
}