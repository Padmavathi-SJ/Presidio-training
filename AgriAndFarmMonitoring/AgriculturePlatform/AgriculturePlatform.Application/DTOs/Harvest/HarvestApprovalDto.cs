// Application/DTOs/Harvest/HarvestApprovalDto.cs
namespace AgriculturePlatform.Application.DTOs.Harvest;

public class HarvestApprovalDto
{
    public int HarvestId { get; set; }
    public string ApprovalStatus { get; set; } = "APPROVED"; // APPROVED, REJECTED, REQUEST_CHANGES
    public string? RejectionReason { get; set; }
    public string? AdminNotes { get; set; }
}

public class HarvestWorkerResponseDto
{
    public int HarvestId { get; set; }
    public string WorkerResponse { get; set; } = string.Empty;
}