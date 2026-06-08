// AgriculturePlatform.Application/DTOs/Worker/WorkerFieldListDto.cs
namespace AgriculturePlatform.Application.DTOs.Worker;

public class WorkerFieldListDto
{
    public int AssignmentId { get; set; }
    public int FieldId { get; set; }
    public string FieldName { get; set; } = string.Empty;
    public string? Location { get; set; }
    public decimal? AreaHectares { get; set; }
    public string? SoilType { get; set; }
    public string? Status { get; set; }
    public DateTime? AssignedDate { get; set; }
    public int ActiveCropCount { get; set; }
}