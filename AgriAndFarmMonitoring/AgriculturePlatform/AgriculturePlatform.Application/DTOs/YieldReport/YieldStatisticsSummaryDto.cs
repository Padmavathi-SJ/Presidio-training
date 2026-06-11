// Application/DTOs/YieldReport/YieldStatisticsSummaryDto.cs
namespace AgriculturePlatform.Application.DTOs.YieldReport;

public class YieldStatisticsSummaryDto
{
    public decimal TotalYieldKg { get; set; }
    public int TotalHarvests { get; set; }
    public decimal AverageYieldPerHarvest { get; set; }
    public decimal TotalValue { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    
    // Computed properties
    public string FormattedTotalYield => $"{TotalYieldKg:N0} kg";
    public string FormattedTotalValue => $"${TotalValue:N2}";
    public int DaysInPeriod => (EndDate - StartDate).Days;
    public decimal AverageDailyYield => DaysInPeriod > 0 ? TotalYieldKg / DaysInPeriod : 0;
}