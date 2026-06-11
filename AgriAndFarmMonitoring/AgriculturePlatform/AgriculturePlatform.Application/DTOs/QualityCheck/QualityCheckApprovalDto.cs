// Application/DTOs/QualityCheck/QualityCheckApprovalDto.cs
namespace AgriculturePlatform.Application.DTOs.QualityCheck;

public class QualityCheckApprovalDto
{
    public int QualityCheckId { get; set; }
    public string ApprovalStatus { get; set; } = "APPROVED";
    public string? RejectionReason { get; set; }
    public string? AdminNotes { get; set; }
}

public class QualityCheckWorkerResponseDto
{
    public int QualityCheckId { get; set; }
    public string WorkerResponse { get; set; } = string.Empty;
}