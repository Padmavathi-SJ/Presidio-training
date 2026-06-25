// AgriculturePlatform.Application/DTOs/Worker/WorkerDto.cs
namespace AgriculturePlatform.Application.DTOs.Worker;

public class WorkerDto
{
    public int Id { get; set; }
    public int FarmId { get; set; }
    public string FarmName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string Role { get; set; } = "Worker";
    public DateTime HireDate { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int? LastLoginDaysAgo { get; set; }
}