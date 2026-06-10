using AgriculturePlatform.Application.DTOs.Observation;
using System.Text.Json;

namespace AgriculturePlatform.API.Services;

public class ObservationStatisticsFormatter
{
    public object FormatForDisplay(ObservationStatisticsDto stats)
    {
        return new
        {
            Summary = new
            {
                stats.TotalObservations,
                stats.ObservationsWithPest,
                stats.ObservationsWithoutPest,
                stats.PestPercentage,
                stats.HealthyPercentage
            },
            PestTypes = stats.PestTypeDistribution
                .OrderByDescending(x => x.Value)
                .Select(x => new { PestType = x.Key, Count = x.Value, Percentage = CalculatePercentage(x.Value, stats.TotalObservations) }),
            CropHealth = stats.CropHealthDistribution.Select(x => new { Health = x.Key, Count = x.Value }),
            TopFields = stats.ObservationsByField
                .OrderByDescending(x => x.Value)
                .Take(5)
                .Select(x => new { FieldName = x.Key, ObservationCount = x.Value, Percentage = CalculatePercentage(x.Value, stats.TotalObservations) }),
            TopWorkers = stats.ObservationsByWorker
                .OrderByDescending(x => x.Value)
                .Take(5)
                .Select(x => new { WorkerName = x.Key, ObservationCount = x.Value, Percentage = CalculatePercentage(x.Value, stats.TotalObservations) }),
            RecentTrend = stats.RecentTrend.Select(t => new
            {
                t.FormattedDate,
                t.TotalCount,
                t.PestCount,
                t.PestPercentage
            })
        };
    }
    
    public object FormatForChartJs(ObservationStatisticsDto stats)
    {
        return new
        {
            PestDistribution = new
            {
                labels = stats.PestTypeDistribution.Keys.ToList(),
                datasets = new[]
                {
                    new
                    {
                        label = "Pest Occurrences",
                        data = stats.PestTypeDistribution.Values.ToList(),
                        backgroundColor = GenerateColors(stats.PestTypeDistribution.Count)
                    }
                }
            },
            CropHealth = new
            {
                labels = stats.CropHealthDistribution.Keys.ToList(),
                datasets = new[]
                {
                    new
                    {
                        label = "Crop Health Status",
                        data = stats.CropHealthDistribution.Values.ToList(),
                        backgroundColor = new[] { "#22c55e", "#84cc16", "#eab308", "#f97316", "#ef4444" }
                    }
                }
            },
            Trend = new
            {
                labels = stats.RecentTrend.Select(t => t.FormattedDate).ToList(),
                datasets = new[]
                {
                    new { label = "Total Observations", data = stats.RecentTrend.Select(t => t.TotalCount).ToList(), borderColor = "#3b82f6", backgroundColor = "rgba(59, 130, 246, 0.1)" },
                    new { label = "Pest Detected", data = stats.RecentTrend.Select(t => t.PestCount).ToList(), borderColor = "#ef4444", backgroundColor = "rgba(239, 68, 68, 0.1)" }
                }
            },
            TopFields = new
            {
                labels = stats.ObservationsByField.OrderByDescending(x => x.Value).Take(5).Select(x => x.Key).ToList(),
                data = stats.ObservationsByField.OrderByDescending(x => x.Value).Take(5).Select(x => x.Value).ToList()
            }
        };
    }
    
    private decimal CalculatePercentage(int count, int total)
    {
        return total > 0 ? Math.Round((count / (decimal)total) * 100, 2) : 0;
    }
    
    private string[] GenerateColors(int count)
    {
        var colors = new[] { "#ef4444", "#f59e0b", "#eab308", "#22c55e", "#06b6d4", "#8b5cf6", "#ec4899", "#14b8a6", "#f97316", "#a855f7" };
        return Enumerable.Range(0, count).Select(i => colors[i % colors.Length]).ToArray();
    }
}