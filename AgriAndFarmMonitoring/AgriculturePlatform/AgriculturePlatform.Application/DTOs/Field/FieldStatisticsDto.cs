// AgriculturePlatform.Application/DTOs/Field/FieldStatisticsDto.cs
namespace AgriculturePlatform.Application.DTOs.Field;

public class FieldStatisticsDto
{
    public int TotalFields { get; set; }
    public int ActiveFields { get; set; }
    public int DeletedFields { get; set; }  // NEW
    public decimal TotalAreaHectares { get; set; }
    public int FallowFields { get; set; }
    public int PreparingFields { get; set; }
    public int MaintenanceFields { get; set; }
    public int RetiredFields { get; set; }
    public int TotalActiveCrops { get; set; }
    public Dictionary<string, int> SoilTypeDistribution { get; set; } = new();
}