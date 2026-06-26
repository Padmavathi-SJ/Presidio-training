// AgriculturePlatform.Application/DTOs/Worker/ResetPasswordDto.cs
namespace AgriculturePlatform.Application.DTOs.Worker;

public class ResetPasswordDto
{
    public string NewPassword { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
}