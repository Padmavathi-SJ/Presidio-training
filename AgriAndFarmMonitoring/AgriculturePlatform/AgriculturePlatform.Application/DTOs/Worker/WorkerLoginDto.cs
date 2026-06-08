// AgriculturePlatform.Application/DTOs/Worker/WorkerLoginDto.cs
namespace AgriculturePlatform.Application.DTOs.Worker;

public class WorkerLoginDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}