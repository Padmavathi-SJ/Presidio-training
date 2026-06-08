// AgriculturePlatform.Application/DTOs/Worker/UpdateWorkerProfileDto.cs
namespace AgriculturePlatform.Application.DTOs.Worker;

public class UpdateWorkerProfileDto
{
    public string? Name { get; set; }
    public string? Phone { get; set; }
    public string? CurrentPassword { get; set; }
    public string? NewPassword { get; set; }
}