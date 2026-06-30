// AgriculturePlatform.Application/DTOs/Task/BulkStatusUpdateExcelDto.cs
namespace AgriculturePlatform.Application.DTOs.Task;

public class BulkStatusUpdateExcelDto
{
    public string TaskName { get; set; } = string.Empty;  // ✅ Changed from TaskId
    public string Status { get; set; } = string.Empty;
}