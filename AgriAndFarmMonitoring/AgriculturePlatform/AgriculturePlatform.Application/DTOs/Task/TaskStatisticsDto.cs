// AgriculturePlatform.Application/DTOs/Task/TaskStatisticsDto.cs
namespace AgriculturePlatform.Application.DTOs.Task;

public class TaskStatisticsDto
{
    public int TotalTasks { get; set; }
    public int PendingTasks { get; set; }
    public int InProgressTasks { get; set; }
    public int CompletedTasks { get; set; }
    public int OverdueTasks { get; set; }
    public int CancelledTasks { get; set; }
    public Dictionary<string, int> TasksByPriority { get; set; } = new();
    public Dictionary<string, int> TasksByType { get; set; } = new();
    public double AverageCompletionTimeDays { get; set; }
}