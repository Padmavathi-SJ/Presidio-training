// Application/DTOs/Harvest/HarvestDto.cs
namespace AgriculturePlatform.Application.DTOs.Harvest;

public class HarvestDto
{
    public int Id { get; set; }
    public int FarmId { get; set; }
    public string FarmName { get; set; } = string.Empty;
    public int FieldId { get; set; }
    public string FieldName { get; set; } = string.Empty;
    public int CropCycleId { get; set; }
    public string CropType { get; set; } = string.Empty;
    public int? HarvestedBy { get; set; }
    public string? HarvesterName { get; set; }
    public int? SubmittedBy { get; set; }
    public string? SubmitterName { get; set; }
    public DateTime HarvestDate { get; set; }
    public decimal QuantityKg { get; set; }
    public string? QualityGrade { get; set; }
    public string? HarvestMethod { get; set; }
    
    // Approval fields
    public string ApprovalStatus { get; set; } = "PENDING";
    public int? ApprovedBy { get; set; }
    public string? ApproverName { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? RejectionReason { get; set; }
    public string? AdminNotes { get; set; }
    public string? WorkerResponse { get; set; }
    
    // Financial fields
    public decimal? PricePerKg { get; set; }
    public decimal? TotalValue { get; set; }
    public string? BatchNumber { get; set; }
    public string? Notes { get; set; }
    
    // Audit fields
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int? CreatedBy { get; set; }
    
    // Image attachment properties
    public string? ImagePath { get; set; }
    public string? ThumbnailPath { get; set; }
    public string? ImageCaption { get; set; }
    public List<string> AdditionalImagePaths { get; set; } = new List<string>();
    public string? ImageMetadata { get; set; }
    
    // Computed properties
    public string StatusBadgeColor => ApprovalStatus switch
    {
        "APPROVED" => "green",
        "PENDING" => "yellow",
        "REJECTED" => "red",
        "REQUEST_CHANGES" => "orange",
        _ => "gray"
    };
    
    public string FormattedQuantity => $"{QuantityKg:N0} kg";
    public string FormattedTotalValue => TotalValue.HasValue ? $"${TotalValue:N2}" : "N/A";
}