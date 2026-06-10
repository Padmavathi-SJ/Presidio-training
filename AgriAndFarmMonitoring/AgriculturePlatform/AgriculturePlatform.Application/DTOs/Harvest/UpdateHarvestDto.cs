// Application/DTOs/Harvest/UpdateHarvestDto.cs
namespace AgriculturePlatform.Application.DTOs.Harvest;

public class UpdateHarvestDto
{
    public DateTime? HarvestDate { get; set; }
    public decimal? QuantityKg { get; set; }
    public string? QualityGrade { get; set; }
    public string? HarvestMethod { get; set; }
    public string? Notes { get; set; }
    public decimal? PricePerKg { get; set; }
    public string? BatchNumber { get; set; }
}