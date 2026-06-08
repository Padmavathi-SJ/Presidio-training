// AgriculturePlatform.Application/DTOs/Worker/WorkerProfileDto.cs
namespace AgriculturePlatform.Application.DTOs.Worker;

public class WorkerProfileDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Role { get; set; }
    public string? FarmName { get; set; }
    public DateTime HireDate { get; set; }
    public bool IsActive { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}