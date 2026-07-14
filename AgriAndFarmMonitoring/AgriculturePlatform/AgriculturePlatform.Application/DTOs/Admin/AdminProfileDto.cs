// AgriculturePlatform.Application/DTOs/Admin/AdminProfileDto.cs
namespace AgriculturePlatform.Application.DTOs.Admin;

public class AdminProfileDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    
    // Farm Details
    public int FarmId { get; set; }
    public string FarmName { get; set; } = string.Empty;
    public string? FarmEmail { get; set; }
    public string? FarmPhone { get; set; }
    public string? FarmAddress { get; set; }
}
