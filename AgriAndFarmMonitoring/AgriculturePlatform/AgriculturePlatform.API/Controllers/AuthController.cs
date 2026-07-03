// AgriculturePlatform.API/Controllers/AuthController.cs (UPDATED)
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AgriculturePlatform.Application.DTOs.Admin;
using AgriculturePlatform.Application.DTOs.Worker;
using AgriculturePlatform.Application.Interfaces;
using AgriculturePlatform.Application.Exceptions;

namespace AgriculturePlatform.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAdminService _adminService;
    private readonly IWorkerAuthService _workerAuthService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IAdminService adminService,
        IWorkerAuthService workerAuthService,
        ILogger<AuthController> logger)
    {
        _adminService = adminService;
        _workerAuthService = workerAuthService;
        _logger = logger;
    }

    private string GetIpAddress()
    {
        var ip = Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (string.IsNullOrEmpty(ip))
            ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        return ip ?? "127.0.0.1";
    }

    // =============================================
    // UNIFIED LOGIN - Handles both Admin and Worker
    // =============================================
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] UnifiedLoginDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var ipAddress = GetIpAddress();
            object result;
            string userType;

            // ✅ Try admin login first
            try
            {
                var adminResult = await _adminService.LoginAsync(
                    new LoginDto { Email = dto.Email, Password = dto.Password }, 
                    ipAddress);
                
                result = adminResult;
                userType = "Admin";
                _logger.LogInformation("Admin logged in: {Email} from IP {Ip}", dto.Email, ipAddress);
            }
            catch (UnauthorizedException)
            {
                // ✅ If admin login fails, try worker login
                try
                {
                    var workerResult = await _workerAuthService.LoginAsync(
                        new WorkerLoginDto { Email = dto.Email, Password = dto.Password }, 
                        ipAddress);
                    
                    result = workerResult;
                    userType = "Worker";
                    _logger.LogInformation("Worker logged in: {Email} from IP {Ip}", dto.Email, ipAddress);
                }
                catch (UnauthorizedException)
                {
                    // Both failed
                    _logger.LogWarning("Login failed for {Email}: Invalid credentials", dto.Email);
                    return Unauthorized(new { 
                        success = false, 
                        message = "Invalid email or password",
                        userType = "Unknown" 
                    });
                }
            }

            return Ok(new { 
                success = true, 
                data = result,
                userType = userType
            });
        }
        catch (BadRequestException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during login for {Email}", dto.Email);
            return StatusCode(500, new { success = false, message = "An error occurred during login" });
        }
    }

    // =============================================
    // UNIFIED REFRESH TOKEN
    // =============================================
    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var ipAddress = GetIpAddress();
            
            // Try to get user type from expired token
            var userType = GetUserTypeFromToken(dto.AccessToken);
            object result;

            if (userType == "Worker")
            {
                result = await _workerAuthService.RefreshTokenAsync(dto, ipAddress);
            }
            else
            {
                result = await _adminService.RefreshTokenAsync(dto, ipAddress);
            }

            _logger.LogInformation("Token refreshed for {UserType} from IP {Ip}", userType, ipAddress);
            return Ok(new { success = true, data = result });
        }
        catch (UnauthorizedException ex)
        {
            _logger.LogWarning("Refresh token failed: {Message}", ex.Message);
            return Unauthorized(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during token refresh");
            return StatusCode(500, new { success = false, message = "An error occurred" });
        }
    }

    // =============================================
    // UNIFIED LOGOUT
    // =============================================
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout([FromBody] RevokeTokenDto? dto)
    {
        try
        {
            var ipAddress = GetIpAddress();
            
            // Determine user type from token
            var userType = GetUserTypeFromToken();
            
            if (userType == "Worker")
            {
                var workerId = int.Parse(User.FindFirst("workerId")?.Value ?? "0");
                await _workerAuthService.RevokeAllUserTokensAsync(workerId, ipAddress);
            }
            else
            {
                var adminId = int.Parse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ?? "0");
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

    // =============================================
    // UNIFIED TOKEN VALIDATION
    // =============================================
    [HttpGet("validate")]
    [Authorize]
    public IActionResult ValidateToken()
    {
        var userType = GetUserTypeFromToken();
        return Ok(new { 
            success = true, 
            message = "Token is valid",
            userType = userType
        });
    }

    // =============================================
    // ADMIN-ONLY REGISTRATION (No change)
    // =============================================
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
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during registration");
            return StatusCode(500, new { success = false, message = "An error occurred during registration" });
        }
    }

    // =============================================
    // CHANGE PASSWORD (For Admin only)
    // =============================================
    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            
            var adminId = GetCurrentAdminId();
            var ipAddress = GetIpAddress();
            
            var result = await _adminService.ChangePasswordAsync(adminId, dto, ipAddress);
            
            if (result)
            {
                _logger.LogInformation("Password changed successfully for admin {AdminId}", adminId);
                return Ok(new { success = true, message = "Password changed successfully" });
            }
            
            return BadRequest(new { success = false, message = "Failed to change password" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during password change");
            return StatusCode(500, new { success = false, message = "An error occurred" });
        }
    }

    // =============================================
    // HELPER METHODS
    // =============================================
    private string GetUserTypeFromToken()
    {
        return User.FindFirst("userType")?.Value ?? 
               User.FindFirst("role")?.Value ?? 
               "Admin";
    }

    private string GetUserTypeFromToken(string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            var userType = jwtToken.Claims.FirstOrDefault(c => c.Type == "userType")?.Value;
            return userType ?? "Admin";
        }
        catch
        {
            return "Admin";
        }
    }

    private int GetCurrentAdminId()
    {
        var adminIdClaim = User.FindFirst("adminId")?.Value ?? 
                           User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                           User.FindFirst("sub")?.Value;
        
        return int.TryParse(adminIdClaim, out var adminId) ? adminId : 0;
    }
}

// =============================================
// UNIFIED LOGIN DTO
// =============================================
public class UnifiedLoginDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}