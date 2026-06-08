// AgriculturePlatform.Application/DTOs/Task/BulkAssignTaskDto.cs
namespace AgriculturePlatform.Application.DTOs.Task;

public class BulkAssignTaskDto
{
    public List<int> WorkerIds { get; set; } = new();
    public int? FieldId { get; set; }
    public int? CropCycleId { get; set; }
    public string TaskName { get; set; } = string.Empty;
    public DateTime? DueDate { get; set; }
    public string? Priority { get; set; } = "MEDIUM";
    public string? Notes { get; set; }
}

public class BulkAssignResultDto
{
    public int TotalRequests { get; set; }
    public int SuccessCount { get; set; }
    public int FailedCount { get; set; }
    public List<BulkAssignError> Errors { get; set; } = new();
}

public class BulkAssignError
{
     public int RowNumber { get; set; }
    public int WorkerId { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
}