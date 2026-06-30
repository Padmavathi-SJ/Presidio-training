// AgriculturePlatform.Application/DTOs/Task/BulkReassignExcelDto.cs
namespace AgriculturePlatform.Application.DTOs.Task;

public class BulkReassignExcelDto
{
    public string TaskName { get; set; } = string.Empty;      // ✅ Changed from TaskId
    public string NewWorkerName { get; set; } = string.Empty; // ✅ Changed from NewWorkerId
}