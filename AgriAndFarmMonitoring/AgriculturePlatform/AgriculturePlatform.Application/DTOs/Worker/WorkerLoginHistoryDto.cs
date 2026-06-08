// AgriculturePlatform.Application/DTOs/Worker/WorkerLoginHistoryDto.cs
namespace AgriculturePlatform.Application.DTOs.Worker;

public class WorkerLoginHistoryDto
{
    public int WorkerId { get; set; }
    public string WorkerName { get; set; } = string.Empty;
    public DateTime? LastLoginAt { get; set; }
    public string? LastLoginIp { get; set; }
    public int TotalLogins { get; set; }
}