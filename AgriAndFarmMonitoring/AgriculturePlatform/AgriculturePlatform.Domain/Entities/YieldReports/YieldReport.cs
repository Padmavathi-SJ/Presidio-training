// Domain/Entities/YieldReports/YieldReport.cs
using AgriculturePlatform.Domain.Enums;
using AgriculturePlatform.Domain.Entities.AdminEntities;
using AgriculturePlatform.Domain.Entities.CropMonitoring;
using AgriculturePlatform.Domain.Common;

namespace AgriculturePlatform.Domain.Entities.YieldReports;

public class YieldReport : BaseEntity
{
    public int FarmId { get; set; }
    public int AdminId { get; set; }
    public int? CropCycleId { get; set; }
    public int? FieldId { get; set; }
    
    // Report details
    public string ReportName { get; set; } = string.Empty;
    public string ReportType { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    
    // Yield statistics
    public decimal TotalYieldKg { get; set; }
    public decimal AverageYieldPerHectare { get; set; }
    public int TotalHarvests { get; set; }
    public decimal AveragePricePerKg { get; set; }
    public decimal TotalValue { get; set; }
    
    // Quality statistics
    public string? AverageQualityGrade { get; set; }
    public decimal PassRate { get; set; }
    public decimal RejectionRate { get; set; }
    
    // Detailed data (stored as JSON)
    public string? FieldBreakdownJson { get; set; }
    public string? CropTypeBreakdownJson { get; set; }
    public string? MonthlyTrendJson { get; set; }
    public string? QualityDistributionJson { get; set; }
    
    // File storage fields - ADD THESE
    public string? FileName { get; set; }
    public string? FilePath { get; set; }
    public string? FileFormat { get; set; }    
    public long? FileSize { get; set; }
    public DateTime? ExportedAt { get; set; }
    public int? ExportedBy { get; set; }
    
    // Scheduling
    public bool IsScheduled { get; set; } = false;
    public string? ScheduleCron { get; set; }
    public DateTime? LastGeneratedAt { get; set; }
    public DateTime? NextScheduledRun { get; set; }
    
    // Navigation properties
    public virtual Farm? Farm { get; set; }
    public virtual Admin? Admin { get; set; }
    public virtual CropCycle? CropCycle { get; set; }
    public virtual Field? Field { get; set; }
    public virtual Admin? Exporter { get; set; }
}