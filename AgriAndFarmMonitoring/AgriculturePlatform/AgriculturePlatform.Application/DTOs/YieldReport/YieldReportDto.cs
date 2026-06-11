// Application/DTOs/YieldReport/YieldReportDto.cs
namespace AgriculturePlatform.Application.DTOs.YieldReport;

public class YieldReportDto
{
    public int Id { get; set; }
    public int FarmId { get; set; }
    public string FarmName { get; set; } = string.Empty;
    public int? CropCycleId { get; set; }
    public string? CropType { get; set; }
    public int? FieldId { get; set; }
    public string? FieldName { get; set; }
    
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
    
    // Breakdown data
    public List<FieldYieldBreakdownDto>? FieldBreakdown { get; set; }
    public List<CropTypeYieldBreakdownDto>? CropTypeBreakdown { get; set; }
    public List<MonthlyYieldTrendDto>? MonthlyTrend { get; set; }
    public List<QualityDistributionDto>? QualityDistribution { get; set; }
    
    // Export info
    public DateTime? ExportedAt { get; set; }
    public string? ExportFormat { get; set; }
    
    // Scheduling
    public bool IsScheduled { get; set; }
    public string? ScheduleCron { get; set; }
    public DateTime? NextScheduledRun { get; set; }
    public DateTime? LastGeneratedAt { get; set; }
    
    // Audit
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Computed properties
    public string FormattedYield => $"{TotalYieldKg:N0} kg";
    public string FormattedValue => $"${TotalValue:N2}";
    public string YieldPerHectareFormatted => $"{AverageYieldPerHectare:N0} kg/ha";
    public string DateRange => $"{StartDate:MMM dd, yyyy} - {EndDate:MMM dd, yyyy}";
}

public class FieldYieldBreakdownDto
{
    public string FieldName { get; set; } = string.Empty;
    public decimal TotalYieldKg { get; set; }
    public decimal YieldPerHectare { get; set; }
    public int HarvestCount { get; set; }
    public decimal PercentageOfTotal { get; set; }
}

public class CropTypeYieldBreakdownDto
{
    public string CropType { get; set; } = string.Empty;
    public decimal TotalYieldKg { get; set; }
    public int HarvestCount { get; set; }
    public decimal AveragePricePerKg { get; set; }
    public decimal TotalValue { get; set; }
    public decimal PercentageOfTotal { get; set; }
}

public class MonthlyYieldTrendDto
{
    public string Month { get; set; } = string.Empty;
    public int Year { get; set; }
    public decimal YieldKg { get; set; }
    public int HarvestCount { get; set; }
    public decimal AveragePrice { get; set; }
    public decimal TotalValue { get; set; }
}

public class QualityDistributionDto
{
    public string Grade { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal Percentage { get; set; }
}