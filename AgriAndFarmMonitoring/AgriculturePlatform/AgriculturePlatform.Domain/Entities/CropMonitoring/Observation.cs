// Domain/Entities/CropMonitoring/Observation.cs
using AgriculturePlatform.Domain.Enums;
using AgriculturePlatform.Domain.Entities.AdminEntities;
using AgriculturePlatform.Domain.Entities.WorkerManagement;
using AgriculturePlatform.Domain.Common;

namespace AgriculturePlatform.Domain.Entities.CropMonitoring;

public class Observation : BaseEntity
{
   
    public int FarmId { get; set; }
    public int AdminId { get; set; }
    public int FieldId { get; set; }
    public int? CropCycleId { get; set; }
    public int? WorkerId { get; set; }
    public DateTime ObservationDate { get; set; } = DateTime.UtcNow;
    public CropHealthEnum? CropHealth { get; set; }
    public bool PestDetected { get; set; } = false;
    public string? PestType { get; set; }
    public string? Notes { get; set; }
   
    
    // Navigation properties
    public virtual Farm? Farm { get; set; }
    public virtual Admin? Admin { get; set; }
    public virtual Field? Field { get; set; }
    public virtual CropCycle? CropCycle { get; set; }
    public virtual Worker? Worker { get; set; }
}