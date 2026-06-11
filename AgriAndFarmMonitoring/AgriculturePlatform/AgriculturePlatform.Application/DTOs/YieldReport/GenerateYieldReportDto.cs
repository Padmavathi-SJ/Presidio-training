// Application/DTOs/YieldReport/GenerateYieldReportDto.cs
namespace AgriculturePlatform.Application.DTOs.YieldReport;

public class GenerateYieldReportDto
{
    public int? CropCycleId { get; set; }
    public int? FieldId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? ReportName { get; set; }
    public string? ExportFormat { get; set; } = "JSON"; // JSON, PDF, EXCEL, CSV
}

public class YieldComparisonDto
{
    public List<FieldComparisonDto> FieldComparisons { get; set; } = new();
    public List<SeasonalComparisonDto> SeasonalComparisons { get; set; } = new();
    public ComparisonSummaryDto Summary { get; set; } = new();
}

public class FieldComparisonDto
{
    public string FieldName { get; set; } = string.Empty;
    public decimal CurrentYield { get; set; }
    public decimal PreviousYield { get; set; }
    public decimal ChangePercentage { get; set; }
    public string Trend { get; set; } = "STABLE"; // INCREASING, DECREASING, STABLE
}

public class SeasonalComparisonDto
{
    public string Season { get; set; } = string.Empty;
    public int Year { get; set; }
    public decimal TotalYield { get; set; }
    public decimal AveragePrice { get; set; }
    public decimal TotalValue { get; set; }
}

public class ComparisonSummaryDto
{
    public decimal OverallChangePercentage { get; set; }
    public string BestPerformingField { get; set; } = string.Empty;
    public decimal BestYield { get; set; }
    public string WorstPerformingField { get; set; } = string.Empty;
    public decimal WorstYield { get; set; }
    public string Recommendation { get; set; } = string.Empty;
}