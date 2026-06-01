using AgriculturePlatform.Domain.Enums;

namespace AgriculturePlatform.Domain.Entities.CropMonitoring;

public class CropCycle
{
    public int Id { get; set; }
    public int FieldId { get; set; }
    public CropTypeEnum? CropType { get; set; }
    public DateTime? PlantingDate { get; set; }
    public DateTime? ExpectedHarvestDate { get; set; }
    public GrowthStageEnum? GrowthStage { get; set; }
    public TaskStatusEnum? Status { get; set; } = TaskStatusEnum.PENDING;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public virtual Field? Field { get; set; }
    public virtual ICollection<SensorReading> SensorReadings { get; set; } = new List<SensorReading>();
    public virtual ICollection<Alert> Alerts { get; set; } = new List<Alert>();
    public virtual ICollection<Observation> Observations { get; set; } = new List<Observation>();
    public virtual ICollection<Task> Tasks { get; set; } = new List<Task>();
    public virtual ICollection<Harvest> Harvests { get; set; } = new List<Harvest>();
    public virtual ICollection<YieldReport> YieldReports { get; set; } = new List<YieldReport>();
}