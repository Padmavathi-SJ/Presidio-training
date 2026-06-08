// AgriculturePlatform.Application/DTOs/Worker/WorkerFilterDto.cs
namespace AgriculturePlatform.Application.DTOs.Worker;

public class WorkerFilterDto
{
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? Role { get; set; }
    public bool? IsActive { get; set; }
    public DateTime? HireDateFrom { get; set; }
    public DateTime? HireDateTo { get; set; }
    public int? Page { get; set; } = 1;
    public int? PageSize { get; set; } = 10;
    public string? SortBy { get; set; } = "CreatedAt";
    public bool IsDescending { get; set; } = true;
    public bool? IncludeDeleted { get; set; } = false;
}