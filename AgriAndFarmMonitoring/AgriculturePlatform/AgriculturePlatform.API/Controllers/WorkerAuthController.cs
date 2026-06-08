// AgriculturePlatform.API/Controllers/WorkerAuthController.cs
using Microsoft.AspNetCore.Mvc;
using AgriculturePlatform.Application.DTOs.Worker;
using AgriculturePlatform.Application.Exceptions;
using AgriculturePlatform.Application.Interfaces;

namespace AgriculturePlatform.API.Controllers;

[ApiController]
[Route("api/worker/auth")]
public class WorkerAuthController : ControllerBase
{
    private readonly IWorkerAuthService _workerAuthService;

    public WorkerAuthController(IWorkerAuthService workerAuthService)
    {
        _workerAuthService = workerAuthService;
    }

    private string GetIpAddress()
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        return string.IsNullOrEmpty(ip) ? "Unknown" : ip;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] WorkerLoginDto dto)
    {
        try
        {
            var ipAddress = GetIpAddress();
            var result = await _workerAuthService.LoginAsync(dto, ipAddress);
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