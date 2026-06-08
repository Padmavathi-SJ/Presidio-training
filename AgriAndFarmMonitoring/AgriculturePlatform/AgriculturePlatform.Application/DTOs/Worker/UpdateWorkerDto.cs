// AgriculturePlatform.Application/DTOs/Worker/UpdateWorkerDto.cs
namespace AgriculturePlatform.Application.DTOs.Worker;

public class UpdateWorkerDto
{
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Role { get; set; }
    public bool? IsActive { get; set; }
}