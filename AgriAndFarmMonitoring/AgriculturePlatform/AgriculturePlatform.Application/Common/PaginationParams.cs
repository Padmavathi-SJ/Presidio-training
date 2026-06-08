// AgriculturePlatform.Application/Common/PaginationParams.cs
namespace AgriculturePlatform.Application.Common;

public class PaginationParams
{
    private const int MaxPageSize = 100;
    private int _pageSize = 10;
    
    public int Page { get; set; } = 1;
    
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value > MaxPageSize ? MaxPageSize : value;
    }
    
    public string? SortBy { get; set; }
    public bool IsDescending { get; set; } = false;
}