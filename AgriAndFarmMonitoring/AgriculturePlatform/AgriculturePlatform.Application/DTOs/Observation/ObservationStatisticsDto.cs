// AgriculturePlatform.Application/DTOs/Observation/ObservationStatisticsDto.cs

namespace AgriculturePlatform.Application.DTOs.Observation;

public class ObservationStatisticsDto
{
    public int TotalObservations { get; set; }
    public int PendingObservations { get; set; }
    public int QuestionedObservations { get; set; }
    public int VerifiedObservations { get; set; }
    public int InvalidObservations { get; set; }
    public int ObservationsWithPest { get; set; }
    public int ObservationsWithoutPest { get; set; }
    public Dictionary<string, int> PestTypeDistribution { get; set; } = new();
    public Dictionary<string, int> CropHealthDistribution { get; set; } = new();
    public Dictionary<string, int> ObservationsByField { get; set; } = new();
    public Dictionary<string, int> ObservationsByWorker { get; set; } = new();
    public List<DailyObservationTrendDto> RecentTrend { get; set; } = new();
    
    // Computed properties for easy display
    public decimal PestPercentage => TotalObservations > 0 
        ? Math.Round((ObservationsWithPest / (decimal)TotalObservations) * 100, 2) 
        : 0;
    
    public decimal HealthyPercentage => TotalObservations > 0 
        ? Math.Round((ObservationsWithoutPest / (decimal)TotalObservations) * 100, 2) 
        : 0;
    
    public List<PestTypeStatDto> TopPestTypes => PestTypeDistribution
        .OrderByDescending(x => x.Value)
        .Take(5)
        .Select(x => new PestTypeStatDto { PestType = x.Key, Count = x.Value, Percentage = CalculatePercentage(x.Value, TotalObservations) })
        .ToList();
    
    public List<FieldStatDto> TopFields => ObservationsByField
        .OrderByDescending(x => x.Value)
        .Take(5)
        .Select(x => new FieldStatDto { FieldName = x.Key, ObservationCount = x.Value, Percentage = CalculatePercentage(x.Value, TotalObservations) })
        .ToList();
    
    public List<WorkerStatDto> TopWorkers => ObservationsByWorker
        .OrderByDescending(x => x.Value)
        .Take(5)
        .Select(x => new WorkerStatDto { WorkerName = x.Key, ObservationCount = x.Value, Percentage = CalculatePercentage(x.Value, TotalObservations) })
        .ToList();
    
    private decimal CalculatePercentage(int count, int total) => total > 0 ? Math.Round((count / (decimal)total) * 100, 2) : 0;
}

public class PestTypeStatDto
{
    public string PestType { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal Percentage { get; set; }
}

public class FieldStatDto
{
    public string FieldName { get; set; } = string.Empty;
    public int ObservationCount { get; set; }
    public decimal Percentage { get; set; }
}

public class WorkerStatDto
{
    public string WorkerName { get; set; } = string.Empty;
    public int ObservationCount { get; set; }
    public decimal Percentage { get; set; }
}

public class DailyObservationTrendDto
{
    public DateTime Date { get; set; }
    public int TotalCount { get; set; }
    public int PestCount { get; set; }
    
    // Computed property for chart display
    public string FormattedDate => Date.ToString("MMM dd, yyyy");
    public decimal PestPercentage => TotalCount > 0 ? Math.Round((PestCount / (decimal)TotalCount) * 100, 2) : 0;
}