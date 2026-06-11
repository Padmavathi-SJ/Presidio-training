// Application/DTOs/QualityCheck/QualityCheckDto.cs
namespace AgriculturePlatform.Application.DTOs.QualityCheck;

public class QualityCheckDto
{
    public int Id { get; set; }
    public int FarmId { get; set; }
    public string FarmName { get; set; } = string.Empty;
    public int HarvestId { get; set; }
    public string? HarvestBatchNumber { get; set; }
    public decimal? HarvestQuantity { get; set; }
    public int? CheckedBy { get; set; }
    public string? CheckerName { get; set; }
    public DateTime CheckDate { get; set; }
    public decimal? MoisturePct { get; set; }
    public decimal? DefectPct { get; set; }
    public string? FinalGrade { get; set; }
    public string? Notes { get; set; }
    
    // Approval fields
    public string ApprovalStatus { get; set; } = "PENDING";
    public int? ApprovedBy { get; set; }
    public string? ApproverName { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? RejectionReason { get; set; }
    public string? AdminNotes { get; set; }
    public string? WorkerResponse { get; set; }
    
    // Computed properties
    public string StatusBadgeColor => ApprovalStatus switch
    {
        "APPROVED" => "green",
        "PENDING" => "yellow",
        "REJECTED" => "red",
        "REQUEST_CHANGES" => "orange",
        _ => "gray"
    };
    
    public bool IsPass => FinalGrade != "REJECTED" && FinalGrade != "D";
    public string QualityStatus => IsPass ? "Pass" : "Fail";
}