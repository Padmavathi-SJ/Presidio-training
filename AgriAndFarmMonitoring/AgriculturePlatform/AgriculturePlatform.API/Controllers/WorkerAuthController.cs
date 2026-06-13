// API/Controllers/WorkerAuthController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AgriculturePlatform.Application.DTOs.Admin;
using AgriculturePlatform.Application.DTOs.Worker;
using AgriculturePlatform.Application.Exceptions;
using AgriculturePlatform.Application.Interfaces;

namespace AgriculturePlatform.API.Controllers;

[ApiController]
[Route("api/worker/auth")]
public class WorkerAuthController : ControllerBase
{
    private readonly IWorkerAuthService _workerAuthService;
    private readonly ILogger<WorkerAuthController> _logger;

    public WorkerAuthController(IWorkerAuthService workerAuthService, ILogger<WorkerAuthController> logger)
    {
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

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] WorkerLoginDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var ipAddress = GetIpAddress();
            var result = await _workerAuthService.LoginAsync(dto, ipAddress);
            _logger.LogInformation("Worker logged in: {Email} from IP {Ip}", dto.Email, ipAddress);
            return Ok(new { success = true, data = result });
        }
        catch (UnauthorizedException ex)
        {
            _logger.LogWarning("Worker login failed for {Email}: {Message}", dto.Email, ex.Message);
            return Unauthorized(new { success = false, message = ex.Message });
        }
        catch (BadRequestException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during worker login");
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
            var result = await _workerAuthService.RefreshTokenAsync(dto, ipAddress);
            _logger.LogInformation("Token refreshed for worker from IP {Ip}", ipAddress);
            return Ok(new { success = true, data = result });
        }
        catch (UnauthorizedException ex)
        {
            _logger.LogWarning("Refresh token failed: {Message}", ex.Message);
            return Unauthorized(new { success = false, message = ex.Message });
        }
        catch (BadRequestException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during token refresh");
            return StatusCode(500, new { success = false, message = "An error occurred" });
        }
    }

    [HttpPost("revoke-token")]
    [Authorize]
    public async Task<IActionResult> RevokeToken([FromBody] RevokeTokenDto? dto)
    {
        try
        {
            var ipAddress = GetIpAddress();
            
            if (string.IsNullOrWhiteSpace(dto?.RefreshToken))
            {
                var workerId = int.Parse(User.FindFirst("workerId")?.Value ?? "0");
                var result = await _workerAuthService.RevokeAllUserTokensAsync(workerId, ipAddress);
                if (result)
                {
                    _logger.LogInformation("All tokens revoked for worker {WorkerId}", workerId);
                    return Ok(new { success = true, message = "All tokens revoked successfully" });
                }
                return BadRequest(new { success = false, message = "Failed to revoke tokens" });
            }

            var revokeResult = await _workerAuthService.RevokeTokenAsync(dto, ipAddress);
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
                await _workerAuthService.RevokeTokenAsync(dto, ipAddress);
            }
            else
            {
                var workerId = int.Parse(User.FindFirst("workerId")?.Value ?? "0");
                await _workerAuthService.RevokeAllUserTokensAsync(workerId, ipAddress);
            }
            
            _logger.LogInformation("Worker logged out from IP {Ip}", ipAddress);
            return Ok(new { success = true, message = "Logged out successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
            return Ok(new { success = true, message = "Logged out successfully" });
        }
    }
}