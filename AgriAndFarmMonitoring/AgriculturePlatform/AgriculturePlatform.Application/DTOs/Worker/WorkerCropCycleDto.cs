// AgriculturePlatform.Application/DTOs/Worker/WorkerCropCycleDto.cs
namespace AgriculturePlatform.Application.DTOs.Worker;

public class WorkerCropCycleDto
{
    public int Id { get; set; }
    public string? CropType { get; set; }
    public DateTime? PlantingDate { get; set; }
    public DateTime? ExpectedHarvestDate { get; set; }
    public string? GrowthStage { get; set; }
    public string? Status { get; set; }
    public int DaysToHarvest { get; set; }
    public int DaysSincePlanting { get; set; }
    public double? GrowthProgressPercent { get; set; }
}