// AgriculturePlatform.Application/DTOs/Task/BulkAssignTaskExcelDto.cs
namespace AgriculturePlatform.Application.DTOs.Task;

public class BulkAssignTaskExcelDto
{
    public int WorkerId { get; set; }
    public int? FieldId { get; set; }
    public int? CropCycleId { get; set; }
    public string TaskName { get; set; } = string.Empty;
    public DateTime? DueDate { get; set; }
    public string? Priority { get; set; }
    public string? Notes { get; set; }
}

public class BulkStatusUpdateExcelDto
{
    public int TaskId { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class BulkReassignExcelDto
{
    public int TaskId { get; set; }
    public int NewWorkerId { get; set; }
}