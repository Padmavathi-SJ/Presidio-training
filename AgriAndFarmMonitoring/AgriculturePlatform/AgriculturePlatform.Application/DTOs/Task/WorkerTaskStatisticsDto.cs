// AgriculturePlatform.Application/DTOs/WorkerTask/WorkerTaskStatisticsDto.cs
namespace AgriculturePlatform.Application.DTOs.WorkerTask;

public class WorkerTaskStatisticsDto
{
    public int TotalTasks { get; set; }
    public int PendingTasks { get; set; }
    public int InProgressTasks { get; set; }
    public int CompletedTasks { get; set; }
    public int OverdueTasks { get; set; }
    public int HighPriorityTasks { get; set; }
    public int UrgentPriorityTasks { get; set; }
    public double CompletionRate { get; set; }
    public double AverageCompletionTimeDays { get; set; }
}