// Application/DTOs/QualityCheck/QualityStatisticsDto.cs
namespace AgriculturePlatform.Application.DTOs.QualityCheck;

public class QualityStatisticsDto
{
    public int TotalChecks { get; set; }
    public int ApprovedChecks { get; set; }
    public int RejectedChecks { get; set; }
    public int PendingChecks { get; set; }
    
    public decimal PassRate => TotalChecks > 0 ? Math.Round((ApprovedChecks / (decimal)TotalChecks) * 100, 2) : 0;
    public decimal RejectionRate => TotalChecks > 0 ? Math.Round((RejectedChecks / (decimal)TotalChecks) * 100, 2) : 0;
    
    public Dictionary<string, int> GradeDistribution { get; set; } = new();
    public List<MonthlyQualityTrendDto> MonthlyTrend { get; set; } = new();
    
    public decimal AverageMoisturePct { get; set; }
    public decimal AverageDefectPct { get; set; }
    public decimal MinMoisturePct { get; set; }
    public decimal MaxMoisturePct { get; set; }
    public decimal MinDefectPct { get; set; }
    public decimal MaxDefectPct { get; set; }
    
    public Dictionary<string, int> QualityByWorker { get; set; } = new();
    public Dictionary<string, int> QualityByHarvest { get; set; } = new();
}

public class MonthlyQualityTrendDto
{
    public string Month { get; set; } = string.Empty;
    public int Year { get; set; }
    public int TotalChecks { get; set; }
    public int PassCount { get; set; }
    public int FailCount { get; set; }
    public decimal PassRate { get; set; }
}