// AgriculturePlatform.Application/DTOs/Admin/LoginDto.cs
namespace AgriculturePlatform.Application.DTOs.Admin;

public class LoginDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}