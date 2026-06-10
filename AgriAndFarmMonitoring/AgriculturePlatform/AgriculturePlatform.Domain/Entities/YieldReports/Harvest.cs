// Domain/Entities/YieldReports/Harvest.cs
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
    public int? SubmittedBy { get; set; }  // Worker who submitted (if different from harvestedBy)
    public DateTime HarvestDate { get; set; }
    public decimal QuantityKg { get; set; }
    public QualityGradeEnum? QualityGrade { get; set; }
    public HarvestMethodEnum? HarvestMethod { get; set; }
    
    // Approval Workflow
    public string ApprovalStatus { get; set; } = "PENDING"; // PENDING, APPROVED, REJECTED, REQUEST_CHANGES
    public int? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? RejectionReason { get; set; }
    public string? AdminNotes { get; set; }
    public string? WorkerResponse { get; set; }
    
    // Additional fields
    public string? Notes { get; set; }
    public decimal? PricePerKg { get; set; }
    public decimal? TotalValue => QuantityKg * PricePerKg;
    public string? BatchNumber { get; set; }
    
    // Navigation properties
    public virtual Farm? Farm { get; set; }
    public virtual Admin? Admin { get; set; }
    public virtual Admin? Approver { get; set; }
    public virtual Field? Field { get; set; }
    public virtual CropCycle? CropCycle { get; set; }
    public virtual Worker? Harvester { get; set; }
    public virtual Worker? Submitter { get; set; }
    public virtual ICollection<QualityCheck> QualityChecks { get; set; } = new List<QualityCheck>();
}