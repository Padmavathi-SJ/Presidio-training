// Domain/Entities/CropMonitoring/CropCycle.cs
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
    public DateTime? ActualHarvestDate { get; set; }  // ✅ NEW: Track actual harvest date
    public GrowthStageEnum? GrowthStage { get; set; }
    public TaskStatusEnum? Status { get; set; } = TaskStatusEnum.PENDING;
    
    // ✅ NEW: Track stage changes for notifications
    public DateTime? LastStageUpdate { get; set; }
    public GrowthStageEnum? PreviousGrowthStage { get; set; }
    
    // ✅ NEW: Flag to control auto-update
    public bool AutoUpdateGrowthStage { get; set; } = true;
    
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

    // ✅ NEW: Computed helper properties (not stored in DB)
    public double? GrowthPercentage
    {
        get
        {
            if (!PlantingDate.HasValue || !ExpectedHarvestDate.HasValue)
                return null;
            
            var now = DateTime.UtcNow.Date;
            var planting = PlantingDate.Value.Date;
            var harvest = ExpectedHarvestDate.Value.Date;
            
            if (now < planting)
                return 0;
            
            if (now >= harvest || ActualHarvestDate.HasValue)
                return 100;
            
            var totalDays = (harvest - planting).Days;
            if (totalDays <= 0)
                return 0;
                
            var elapsedDays = (now - planting).Days;
            return Math.Min(100, (double)elapsedDays / totalDays * 100);
        }
    }

    public int? DaysUntilHarvest
    {
        get
        {
            if (!ExpectedHarvestDate.HasValue)
                return null;
            
            if (ActualHarvestDate.HasValue)
                return 0;
                
            var now = DateTime.UtcNow.Date;
            var harvest = ExpectedHarvestDate.Value.Date;
            return (harvest - now).Days;
        }
    }

    public bool IsOverdue
    {
        get
        {
            if (!ExpectedHarvestDate.HasValue || ActualHarvestDate.HasValue)
                return false;
                
            return DateTime.UtcNow.Date > ExpectedHarvestDate.Value.Date;
        }
    }

    /// <summary>
    /// Calculates the growth stage based on the percentage of growth cycle completed
    /// </summary>
    public GrowthStageEnum CalculateGrowthStage()
    {
        // If already harvested, return HARVESTED
        if (ActualHarvestDate.HasValue)
            return GrowthStageEnum.HARVESTED;
        
        // If no planting date, return SEEDLING (default)
        if (!PlantingDate.HasValue)
            return GrowthStageEnum.SEEDLING;
        
        var now = DateTime.UtcNow.Date;
        var planting = PlantingDate.Value.Date;
        
        // If planting is in the future
        if (now < planting)
            return GrowthStageEnum.PLANTED;
        
        // If no harvest date or harvest is in the future, calculate percentage
        var harvest = ExpectedHarvestDate ?? planting.AddDays(120);
        var harvestDate = harvest.Date;
        
        // If harvest is in the past, return HARVESTED
        if (now > harvestDate)
            return GrowthStageEnum.HARVESTED;
        
        // Calculate percentage of growth cycle completed
        var totalDays = (harvestDate - planting).Days;
        if (totalDays <= 0)
            return GrowthStageEnum.SEEDLING;
            
        var elapsedDays = (now - planting).Days;
        var percentage = (double)elapsedDays / totalDays * 100;
        
        // Map percentage to growth stage
        return percentage switch
        {
            < 0 => GrowthStageEnum.PLANTED,
            < 10 => GrowthStageEnum.GERMINATION,
            < 25 => GrowthStageEnum.SEEDLING,
            < 50 => GrowthStageEnum.VEGETATIVE,
            < 70 => GrowthStageEnum.FLOWERING,
            < 85 => GrowthStageEnum.FRUITING,
            < 100 => GrowthStageEnum.MATURE,
            _ => GrowthStageEnum.READY_FOR_HARVEST
        };
    }

    /// <summary>
    /// Updates the growth stage and tracks changes
    /// </summary>
    public bool UpdateGrowthStage(bool forceUpdate = false)
    {
        if (!AutoUpdateGrowthStage && !forceUpdate)
            return false;
            
        var newStage = CalculateGrowthStage();
        
        // If stage changed or forced update
        if (forceUpdate || newStage != GrowthStage)
        {
            PreviousGrowthStage = GrowthStage;
            GrowthStage = newStage;
            LastStageUpdate = DateTime.UtcNow;
            return true;
        }
        
        return false;
    }

    /// <summary>
    /// Marks the crop as harvested
    /// </summary>
    public void MarkAsHarvested(DateTime? harvestDate = null)
    {
        ActualHarvestDate = harvestDate ?? DateTime.UtcNow;
        Status = TaskStatusEnum.COMPLETED;
        GrowthStage = GrowthStageEnum.HARVESTED;
        LastStageUpdate = DateTime.UtcNow;
    }
}