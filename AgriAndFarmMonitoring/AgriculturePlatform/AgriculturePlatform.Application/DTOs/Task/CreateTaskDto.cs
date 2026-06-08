// AgriculturePlatform.Application/DTOs/Task/CreateTaskDto.cs
namespace AgriculturePlatform.Application.DTOs.Task;

public class CreateTaskDto
{
    public int WorkerId { get; set; }
    public int? FieldId { get; set; }
    public int? CropCycleId { get; set; }
    public string TaskName { get; set; } = string.Empty;
    public DateTime? DueDate { get; set; }
    public string? Priority { get; set; } = "MEDIUM";
    public string? Notes { get; set; }
}