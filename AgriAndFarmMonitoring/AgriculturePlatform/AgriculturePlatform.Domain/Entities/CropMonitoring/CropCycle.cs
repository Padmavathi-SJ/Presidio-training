using AgriculturePlatform.Domain.Enums;
using AgriculturePlatform.Domain.Entities.AdminEntities;
using AgriculturePlatform.Domain.Entities.YieldReports;
using AgriculturePlatform.Domain.Entities.WorkerManagement;
using AgriculturePlatform.Domain.Common;

namespace AgriculturePlatform.Domain.Entities.CropMonitoring;

public class CropCycle : BaseEntity

{
    public int FarmId { get; set; }
    public int AdminId { get; set; }
    public int FieldId { get; set; }
    public CropTypeEnum? CropType { get; set; }
    public DateTime? PlantingDate { get; set; }
    public DateTime? ExpectedHarvestDate { get; set; }
    public GrowthStageEnum? GrowthStage { get; set; }
    public TaskStatusEnum? Status { get; set; } = TaskStatusEnum.PENDING;
   
    
    // Navigation properties
    public virtual Farm? Farm { get; set; }
    public virtual Admin? Admin { get; set; }
    public virtual Field? Field { get; set; }
    public virtual ICollection<SensorReading> SensorReadings { get; set; } = new List<SensorReading>();
    public virtual ICollection<Alert> Alerts { get; set; } = new List<Alert>();
    public virtual ICollection<Observation> Observations { get; set; } = new List<Observation>();
    public virtual ICollection<WorkerTask> Tasks { get; set; } = new List<WorkerTask>();
    public virtual ICollection<Harvest> Harvests { get; set; } = new List<Harvest>();
    public virtual ICollection<YieldReport> YieldReports { get; set; } = new List<YieldReport>();
}