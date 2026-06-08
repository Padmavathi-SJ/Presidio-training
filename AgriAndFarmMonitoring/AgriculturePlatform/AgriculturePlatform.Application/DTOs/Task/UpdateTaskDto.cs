// AgriculturePlatform.Application/DTOs/Task/UpdateTaskDto.cs
namespace AgriculturePlatform.Application.DTOs.Task;

public class UpdateTaskDto
{
    public int? WorkerId { get; set; }
    public int? FieldId { get; set; }
    public int? CropCycleId { get; set; }
    public string? TaskName { get; set; }
    public DateTime? DueDate { get; set; }
    public string? Status { get; set; }
    public string? Priority { get; set; }
    public string? Notes { get; set; }
}