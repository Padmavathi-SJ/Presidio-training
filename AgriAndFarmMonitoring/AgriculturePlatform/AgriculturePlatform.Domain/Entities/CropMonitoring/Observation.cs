using AgriculturePlatform.Domain.Enums;

namespace AgriculturePlatform.Domain.Entities.CropMonitoring;

public class Observation
{
    public int Id { get; set; }
    public int FieldId { get; set; }
    public int? CropCycleId { get; set; }
    public int? WorkerId { get; set; }
    public DateTime ObservationDate { get; set; } = DateTime.UtcNow;
    public CropHealthEnum? CropHealth { get; set; }
    public bool PestDetected { get; set; } = false;
    public string? PestType { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual Field? Field { get; set; }
    public virtual CropCycle? CropCycle { get; set; }
    public virtual Worker? Worker { get; set; }
}