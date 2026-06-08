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
    public string? ApprovalStatus { get; set; } = "PENDING";
     public int? ApprovedBy { get; set; }  
    public DateTime? ApprovedAt { get; set; }
    
    // Navigation properties
    public virtual Farm? Farm { get; set; }
    public virtual Admin? Admin { get; set; }
    public virtual Admin? Approver { get; set; } 
    public virtual Harvest? Harvest { get; set; }
    public virtual Worker? Checker { get; set; }
}