// AgriculturePlatform.Domain/Entities/WorkerManagement/WorkerFieldAssignment.cs
using AgriculturePlatform.Domain.Common;
using AgriculturePlatform.Domain.Entities.AdminEntities;
using AgriculturePlatform.Domain.Entities.CropMonitoring;

namespace AgriculturePlatform.Domain.Entities.WorkerManagement;

public class WorkerFieldAssignment : BaseEntity
{
    public int FarmId { get; set; }
    public int AdminId { get; set; }
    public int WorkerId { get; set; }
    public int FieldId { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? AssignedDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Notes { get; set; }

    // Navigation properties
    public virtual Farm? Farm { get; set; }
    public virtual Admin? Admin { get; set; }
    public virtual Worker? Worker { get; set; }
    public virtual Field? Field { get; set; }
}