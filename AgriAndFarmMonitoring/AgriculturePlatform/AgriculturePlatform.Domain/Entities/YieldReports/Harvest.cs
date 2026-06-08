using AgriculturePlatform.Domain.Enums;
using AgriculturePlatform.Domain.Entities.AdminEntities;
using AgriculturePlatform.Domain.Entities.CropMonitoring;
using AgriculturePlatform.Domain.Entities.WorkerManagement;
using AgriculturePlatform.Domain.Common;

namespace AgriculturePlatform.Domain.Entities.YieldReports;

public class Harvest : BaseEntity
{
    public int FarmId { get; set; }
    public int AdminId { get; set; }
    public int FieldId { get; set; }
    public int CropCycleId { get; set; }
    public int? HarvestedBy { get; set; }
    public DateTime HarvestDate { get; set; }
    public decimal QuantityKg { get; set; }
    public QualityGradeEnum? QualityGrade { get; set; }
    public string? ApprovalStatus { get; set; } = "PENDING";
    public int? ApprovedBy { get; set; }  // ADDED
    public DateTime? ApprovedAt { get; set; }  // ADDED
    public string? Notes { get; set; }
    
    // Navigation properties
    public virtual Farm? Farm { get; set; }
    public virtual Admin? Admin { get; set; }
    public virtual Admin? Approver { get; set; }
    public virtual Field? Field { get; set; }
    public virtual CropCycle? CropCycle { get; set; }
    public virtual Worker? Harvester { get; set; }
    public virtual ICollection<QualityCheck> QualityChecks { get; set; } = new List<QualityCheck>();
}