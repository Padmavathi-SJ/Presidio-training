// AgriculturePlatform.Application/DTOs/WorkerTask/UpdateWorkerTaskStatusDto.cs
namespace AgriculturePlatform.Application.DTOs.WorkerTask;

public class UpdateWorkerTaskStatusDto
{
    public string Status { get; set; } = string.Empty;
    public string? CompletionNotes { get; set; }
}