// AgriculturePlatform.Application/DTOs/Field/FieldFilterDto.cs
namespace AgriculturePlatform.Application.DTOs.Field;

public class FieldFilterDto
{
    public string? FieldName { get; set; }
    public string? Location { get; set; }
    public string? SoilType { get; set; }
    public string? Status { get; set; }
    public bool? IncludeDeleted { get; set; } = false;  // NEW - Soft delete filter
    public int? Page { get; set; } = 1;
    public int? PageSize { get; set; } = 10;
    public string? SortBy { get; set; } = "CreatedAt";
    public bool IsDescending { get; set; } = true;
}