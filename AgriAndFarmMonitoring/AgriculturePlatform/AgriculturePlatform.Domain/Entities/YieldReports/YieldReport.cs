using AgriculturePlatform.Domain.Enums;

namespace AgriculturePlatform.Domain.Entities.YieldReports;

public class YieldReport
{
    public int Id { get; set; }
    public int CropCycleId { get; set; }
    public DateTime ReportDate { get; set; } = DateTime.UtcNow;
    public ReportTypeEnum? ReportType { get; set; } = ReportTypeEnum.SEASONAL;
    public decimal? TotalYieldKg { get; set; }
    public decimal? YieldPerHectareKg { get; set; }
    public QualityGradeEnum? AvgQualityGrade { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual CropCycle? CropCycle { get; set; }
}