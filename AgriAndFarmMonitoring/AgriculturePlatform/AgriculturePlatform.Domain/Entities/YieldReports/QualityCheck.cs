using AgriculturePlatform.Domain.Enums;
using AgriculturePlatform.Domain.Entities.AdminEntities;
using AgriculturePlatform.Domain.Entities.WorkerManagement;
using AgriculturePlatform.Domain.Common;

namespace AgriculturePlatform.Domain.Entities.YieldReports;

public class QualityCheck : BaseEntity
{
    public int FarmId { get; set; }
    public int AdminId { get; set; }
    public int HarvestId { get; set; }
    public int? CheckedBy { get; set; }
    public DateTime CheckDate { get; set; } = DateTime.UtcNow;
    public decimal? MoisturePct { get; set; }
    public decimal? DefectPct { get; set; }
    public QualityGradeEnum? FinalGrade { get; set; }
    
    // Approval Workflow - Enhanced
    public string ApprovalStatus { get; set; } = "PENDING"; // PENDING, APPROVED, REJECTED, REQUEST_CHANGES
    public int? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? RejectionReason { get; set; }  // Added for detailed rejection feedback
    public string? AdminNotes { get; set; }       // Added for admin comments
    public string? WorkerResponse { get; set; }   // Added for worker responses
    public string? Notes { get; set; }            // General notes about the quality check
    
    // Navigation properties
    public virtual Farm? Farm { get; set; }
    public virtual Admin? Admin { get; set; }
    public virtual Admin? Approver { get; set; }
    public virtual Harvest? Harvest { get; set; }
    public virtual Worker? Checker { get; set; }
}