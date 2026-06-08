// AgriculturePlatform.Application/DTOs/WorkerField/WorkerFieldAssignmentDto.cs
namespace AgriculturePlatform.Application.DTOs.WorkerField;

public class WorkerFieldAssignmentDto
{
    public int Id { get; set; }
    public int WorkerId { get; set; }
    public string WorkerName { get; set; } = string.Empty;
    public string WorkerEmail { get; set; } = string.Empty;
    public int FieldId { get; set; }
    public string FieldName { get; set; } = string.Empty;
    public string? FieldLocation { get; set; }
    public decimal? FieldAreaHectares { get; set; }
    public string? FieldSoilType { get; set; }
    public bool IsActive { get; set; }
    public DateTime? AssignedDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}