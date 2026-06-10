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
    
    // ===== NEW VALIDATION & COMMENTS FIELDS =====
    public string ValidationStatus { get; set; } = "pending";  // pending, verified, questioned, invalid
    public string? AdminNotes { get; set; }       // Admin's questions/comments on the observation
    public string? WorkerResponse { get; set; }   // Worker's response to admin questions
    public int? ValidatedBy { get; set; }         // Admin ID who validated
    public DateTime? ValidatedAt { get; set; }    // When validation occurred
    public string? FlagReason { get; set; }       // outlier, inconsistent_data, missing_info, duplicate
    
    // Navigation properties
    public virtual Farm? Farm { get; set; }
    public virtual Admin? Admin { get; set; }
    public virtual Field? Field { get; set; }
    public virtual CropCycle? CropCycle { get; set; }
    public virtual Worker? Worker { get; set; }
    public virtual Admin? Validator { get; set; } // Navigation for validator
}