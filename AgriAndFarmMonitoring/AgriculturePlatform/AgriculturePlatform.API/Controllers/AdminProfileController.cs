// AgriculturePlatform.API/Controllers/AdminProfileController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using AgriculturePlatform.Application.DTOs.Admin;
using AgriculturePlatform.Application.Interfaces;
using AgriculturePlatform.API.Filters;

namespace AgriculturePlatform.API.Controllers;

[ApiController]
[Route("api/admin/profile")]
[Authorize]
[AuthorizeFarm]
public class AdminProfileController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminProfileController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    private int GetCurrentAdminId()
    {
        var adminIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                        ?? User.FindFirst("sub")?.Value 
                        ?? User.FindFirst("id")?.Value
                        ?? User.FindFirst("adminId")?.Value;
        
        if (string.IsNullOrEmpty(adminIdClaim))
        {
            return 0;
        }
        
        return int.Parse(adminIdClaim);
    }

    [HttpGet]
    public async Task<IActionResult> GetProfile()
    {
        var adminId = GetCurrentAdminId();
        if (adminId == 0) return Unauthorized(new { message = "Invalid admin token" });

        var result = await _adminService.GetProfileAsync(adminId);
        if (!result.Success) return BadRequest(result);

        return Ok(result);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateAdminProfileDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var adminId = GetCurrentAdminId();
        if (adminId == 0) return Unauthorized(new { message = "Invalid admin token" });

        var result = await _adminService.UpdateProfileAsync(adminId, dto);
        if (!result.Success) return BadRequest(result);

        return Ok(result);
    }
}
