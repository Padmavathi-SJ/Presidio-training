// AgriculturePlatform.Application/DTOs/Admin/RegisterDto.cs
namespace AgriculturePlatform.Application.DTOs.Admin;

public class RegisterDto
{
    // Farm Information
    public string FarmName { get; set; } = string.Empty;
    public string FarmEmail { get; set; } = string.Empty;
    public string? FarmPhone { get; set; }
    public string? FarmAddress { get; set; }
    public string? FarmCity { get; set; }
    public string? FarmState { get; set; }
    public string? FarmCountry { get; set; }
    public string? FarmPostalCode { get; set; }
    public decimal? TotalLandHectares { get; set; }
    
    // Admin Information
    public string AdminName { get; set; } = string.Empty;
    public string AdminEmail { get; set; } = string.Empty;
    public string AdminPassword { get; set; } = string.Empty;
    public string? AdminPhone { get; set; }
}