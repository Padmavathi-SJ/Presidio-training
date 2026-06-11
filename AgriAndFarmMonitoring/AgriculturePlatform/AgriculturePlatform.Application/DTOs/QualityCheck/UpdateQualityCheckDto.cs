// Application/DTOs/QualityCheck/UpdateQualityCheckDto.cs
namespace AgriculturePlatform.Application.DTOs.QualityCheck;

public class UpdateQualityCheckDto
{
    public DateTime? CheckDate { get; set; }
    public decimal? MoisturePct { get; set; }
    public decimal? DefectPct { get; set; }
    public string? FinalGrade { get; set; }
    public string? Notes { get; set; }
}