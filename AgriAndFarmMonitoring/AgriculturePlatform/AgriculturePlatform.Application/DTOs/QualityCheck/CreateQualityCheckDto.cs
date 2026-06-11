// Application/DTOs/QualityCheck/CreateQualityCheckDto.cs
namespace AgriculturePlatform.Application.DTOs.QualityCheck;

public class CreateQualityCheckDto
{
    public int HarvestId { get; set; }
    public DateTime CheckDate { get; set; }
    public decimal? MoisturePct { get; set; }
    public decimal? DefectPct { get; set; }
    public string? FinalGrade { get; set; }
    public string? Notes { get; set; }
}