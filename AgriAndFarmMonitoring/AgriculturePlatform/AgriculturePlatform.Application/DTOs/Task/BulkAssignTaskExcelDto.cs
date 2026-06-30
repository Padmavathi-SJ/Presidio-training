// AgriculturePlatform.Application/DTOs/Task/BulkAssignTaskExcelDto.cs
namespace AgriculturePlatform.Application.DTOs.Task;

public class BulkAssignTaskExcelDto
{
    public string WorkerName { get; set; } = string.Empty;  // ✅ Changed from WorkerId
    public string FieldName { get; set; } = string.Empty;    // ✅ Changed from FieldId
    public string CropCycleName { get; set; } = string.Empty; // ✅ Changed from CropCycleId
    public string TaskName { get; set; } = string.Empty;
    public DateTime? DueDate { get; set; }
    public string? Priority { get; set; }
    public string? Notes { get; set; }
}