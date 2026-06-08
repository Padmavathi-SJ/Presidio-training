// AgriculturePlatform.API/Controllers/AuthController.cs
using Microsoft.AspNetCore.Mvc;
using AgriculturePlatform.Application.DTOs.Admin;
using AgriculturePlatform.Application.Interfaces;
using AgriculturePlatform.Application.Exceptions;

namespace AgriculturePlatform.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AuthController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        try
        {
            var result = await _adminService.RegisterAsync(dto);
            return Ok(result);
        }
        catch (BadRequestException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred during registration", detail = ex.Message });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        try
        {
            var result = await _adminService.LoginAsync(dto);
            return Ok(result);
        }
        catch (UnauthorizedException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred during login", detail = ex.Message });
        }
    }
}