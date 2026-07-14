// AgriculturePlatform.Application/DTOs/Admin/UpdateAdminProfileDto.cs
namespace AgriculturePlatform.Application.DTOs.Admin;

public class UpdateAdminProfileDto
{
    public string? Name { get; set; }
    public string? Phone { get; set; }
    
    // Farm Details
    public string? FarmName { get; set; }
    public string? FarmPhone { get; set; }
    public string? FarmAddress { get; set; }
}
