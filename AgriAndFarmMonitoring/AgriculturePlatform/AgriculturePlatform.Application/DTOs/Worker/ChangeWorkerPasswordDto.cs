// AgriculturePlatform.Application/DTOs/Worker/ChangeWorkerPasswordDto.cs
namespace AgriculturePlatform.Application.DTOs.Worker;

public class ChangeWorkerPasswordDto
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}