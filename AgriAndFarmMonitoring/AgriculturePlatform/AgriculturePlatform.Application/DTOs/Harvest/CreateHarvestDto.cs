// Application/DTOs/Harvest/CreateHarvestDto.cs
namespace AgriculturePlatform.Application.DTOs.Harvest;

public class CreateHarvestDto
{
    public int FieldId { get; set; }
    public int CropCycleId { get; set; }
    public DateTime HarvestDate { get; set; }
    public decimal QuantityKg { get; set; }
    public string? QualityGrade { get; set; }
    public string? HarvestMethod { get; set; }
    public string? Notes { get; set; }
    public decimal? PricePerKg { get; set; }
    public string? BatchNumber { get; set; }
}