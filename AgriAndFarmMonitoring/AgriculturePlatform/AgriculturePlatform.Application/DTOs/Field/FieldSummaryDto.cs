// AgriculturePlatform.Application/DTOs/Field/FieldSummaryDto.cs
namespace AgriculturePlatform.Application.DTOs.Field;

public class FieldSummaryDto
{
    public int Id { get; set; }
    public string FieldName { get; set; } = string.Empty;
    public decimal? AreaHectares { get; set; }
    public string? Status { get; set; }
}
