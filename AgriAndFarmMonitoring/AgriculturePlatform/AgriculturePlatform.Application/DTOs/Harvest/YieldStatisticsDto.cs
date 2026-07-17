// Application/DTOs/Harvest/YieldStatisticsDto.cs
namespace AgriculturePlatform.Application.DTOs.Harvest;

public class YieldStatisticsDto
{
    public decimal TotalYieldKg { get; set; }
    public decimal AverageYieldPerHectare { get; set; }
    public int TotalHarvests { get; set; }
    public int PendingHarvests { get; set; }
    public int ApprovedHarvests { get; set; }
    public int RejectedHarvests { get; set; }
    public int ChangesRequestedHarvests { get; set; }
    public Dictionary<string, decimal> YieldByField { get; set; } = new();
    public Dictionary<string, decimal> YieldByCropType { get; set; } = new();
    public List<MonthlyYieldDto> MonthlyTrend { get; set; } = new();
    public Dictionary<string, int> QualityDistribution { get; set; } = new();
    public Dictionary<string, int> HarvestMethodDistribution { get; set; } = new();
    public decimal TotalValue { get; set; }
    public decimal AveragePricePerKg { get; set; }
    
    // Previous season comparison
    public decimal PreviousSeasonYield { get; set; }
    public decimal YieldGrowthPercentage { get; set; }
}

public class MonthlyYieldDto
{
    public string Month { get; set; } = string.Empty;
    public int Year { get; set; }
    public decimal YieldKg { get; set; }
    public int HarvestCount { get; set; }
    public decimal AveragePrice { get; set; }
}