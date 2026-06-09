// AgriculturePlatform.Application/DTOs/Observation/ObservationStatisticsDto.cs
namespace AgriculturePlatform.Application.DTOs.Observation;

public class ObservationStatisticsDto
{
    public int TotalObservations { get; set; }
    public int ObservationsWithPest { get; set; }
    public int ObservationsWithoutPest { get; set; }
    public Dictionary<string, int> PestTypeDistribution { get; set; } = new();
    public Dictionary<string, int> CropHealthDistribution { get; set; } = new();
    public Dictionary<string, int> ObservationsByField { get; set; } = new();
    public Dictionary<string, int> ObservationsByWorker { get; set; } = new();
    public List<DailyObservationTrendDto> RecentTrend { get; set; } = new();
}

public class DailyObservationTrendDto
{
    public DateTime Date { get; set; }
    public int TotalCount { get; set; }
    public int PestCount { get; set; }
}