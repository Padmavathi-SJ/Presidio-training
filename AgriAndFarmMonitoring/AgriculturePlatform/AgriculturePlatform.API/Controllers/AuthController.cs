using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using AgriculturePlatform.Application.DTOs.Admin;
using AgriculturePlatform.Application.Interfaces;
using AgriculturePlatform.Application.Exceptions;

namespace AgriculturePlatform.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAdminService _adminService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAdminService adminService, ILogger<AuthController> logger)
    {
        _adminService = adminService;
        _logger = logger;
    }

    private string GetIpAddress()
    {
        var ip = Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (string.IsNullOrEmpty(ip))
            ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        return ip ?? "127.0.0.1";
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _adminService.RegisterAsync(dto);
            _logger.LogInformation("New registration: {Email} for farm {FarmName}", dto.AdminEmail, dto.FarmName);
            return Ok(new { success = true, data = result });
        }
        catch (BadRequestException ex)
        {
            _logger.LogWarning("Registration failed: {Message}", ex.Message);
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during registration");
            return StatusCode(500, new { success = false, message = "An error occurred during registration" });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var ipAddress = GetIpAddress();
            var result = await _adminService.LoginAsync(dto, ipAddress);
            _logger.LogInformation("User logged in: {Email} from IP {Ip}", dto.Email, ipAddress);
            return Ok(new { success = true, data = result });
        }
        catch (UnauthorizedException ex)
        {
            _logger.LogWarning("Login failed for {Email}: {Message}", dto.Email, ex.Message);
            return Unauthorized(new { success = false, message = ex.Message });
        }
        catch (BadRequestException ex)
        {
            _logger.LogWarning("Login validation failed: {Message}", ex.Message);
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during login for {Email}", dto.Email);
            return StatusCode(500, new { success = false, message = "An error occurred during login" });
        }
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var ipAddress = GetIpAddress();
            var result = await _adminService.RefreshTokenAsync(dto, ipAddress);
            _logger.LogInformation("Token refreshed for user from IP {Ip}", ipAddress);
            return Ok(new { success = true, data = result });
        }
        catch (UnauthorizedException ex)
        {
            _logger.LogWarning("Refresh token failed: {Message}", ex.Message);
            return Unauthorized(new { success = false, message = ex.Message });
        }
        catch (BadRequestException ex)
        {
            _logger.LogWarning("Refresh token validation failed: {Message}", ex.Message);
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during token refresh");
            return StatusCode(500, new { success = false, message = "An error occurred during token refresh" });
        }
    }

    [HttpPost("revoke-token")]
    [Authorize]
    public async Task<IActionResult> RevokeToken([FromBody] RevokeTokenDto? dto)
    {
        try
        {
            var ipAddress = GetIpAddress();
            
            // If no token provided, revoke current user's all tokens
            if (string.IsNullOrWhiteSpace(dto?.RefreshToken))
            {
                var adminIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ?? 
                                   User.FindFirst("sub")?.Value ?? "0";
                var adminId = int.Parse(adminIdClaim);
                var result = await _adminService.RevokeAllUserTokensAsync(adminId, ipAddress);
                if (result)
                {
                    _logger.LogInformation("All tokens revoked for user {AdminId} from IP {Ip}", adminId, ipAddress);
                    return Ok(new { success = true, message = "All tokens revoked successfully" });
                }
                return BadRequest(new { success = false, message = "Failed to revoke tokens" });
            }

            // Revoke specific token
            var revokeResult = await _adminService.RevokeTokenAsync(dto, ipAddress);
            if (revokeResult)
            {
                _logger.LogInformation("Token revoked from IP {Ip}", ipAddress);
                return Ok(new { success = true, message = "Token revoked successfully" });
            }

            return BadRequest(new { success = false, message = "Token not found or already revoked" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking token");
            return StatusCode(500, new { success = false, message = "An error occurred" });
        }
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout([FromBody] RevokeTokenDto? dto)
    {
        try
        {
            var ipAddress = GetIpAddress();
            
            if (!string.IsNullOrWhiteSpace(dto?.RefreshToken))
            {
                await _adminService.RevokeTokenAsync(dto, ipAddress);
            }
            else
            {
                var adminIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ?? 
                                   User.FindFirst("sub")?.Value ?? "0";
                var adminId = int.Parse(adminIdClaim);
                await _adminService.RevokeAllUserTokensAsync(adminId, ipAddress);
            }
            
            _logger.LogInformation("User logged out from IP {Ip}", ipAddress);
            return Ok(new { success = true, message = "Logged out successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
            return Ok(new { success = true, message = "Logged out successfully" });
        }
    }

    [HttpGet("validate")]
    [Authorize]
    public IActionResult ValidateToken()
    {
        return Ok(new { success = true, message = "Token is valid" });
    }
}