// AgriculturePlatform.Application/DTOs/Worker/CreateWorkerDto.cs
namespace AgriculturePlatform.Application.DTOs.Worker;

public class CreateWorkerDto
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Role { get; set; }
    public string? Password { get; set; }
    public DateTime? HireDate { get; set; }
}