// AgriculturePlatform.Application/DTOs/Admin/AuthResponseDto.cs
namespace AgriculturePlatform.Application.DTOs.Admin;

public class AuthResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public int FarmId { get; set; }
    public string FarmName { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}