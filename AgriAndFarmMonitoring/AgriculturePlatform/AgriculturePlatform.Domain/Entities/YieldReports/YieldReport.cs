// Domain/Entities/YieldReports/YieldReport.cs
using AgriculturePlatform.Domain.Enums;
using AgriculturePlatform.Domain.Entities.AdminEntities;
using AgriculturePlatform.Domain.Entities.CropMonitoring;

namespace AgriculturePlatform.Domain.Entities.YieldReports;

public class YieldReport
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public int AdminId { get; set; }
    public int CropCycleId { get; set; }
    public DateTime ReportDate { get; set; } = DateTime.UtcNow;
    public ReportTypeEnum? ReportType { get; set; } = ReportTypeEnum.SEASONAL;
    public decimal? TotalYieldKg { get; set; }
    public decimal? YieldPerHectareKg { get; set; }
    public QualityGradeEnum? AvgQualityGrade { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public virtual Company? Company { get; set; }
    public virtual Admin? Admin { get; set; }
    public virtual CropCycle? CropCycle { get; set; }
}