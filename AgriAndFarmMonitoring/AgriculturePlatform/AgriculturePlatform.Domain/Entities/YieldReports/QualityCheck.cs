using AgriculturePlatform.Domain.Enums;

namespace AgriculturePlatform.Domain.Entities.YieldReports;

public class QualityCheck
{
    public int Id { get; set; }
    public int HarvestId { get; set; }
    public int? CheckedBy { get; set; }
    public DateTime CheckDate { get; set; } = DateTime.UtcNow;
    public decimal? MoisturePct { get; set; }
    public decimal? DefectPct { get; set; }
    public QualityGradeEnum? FinalGrade { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public virtual Harvest? Harvest { get; set; }
    public virtual Worker? Checker { get; set; }
}