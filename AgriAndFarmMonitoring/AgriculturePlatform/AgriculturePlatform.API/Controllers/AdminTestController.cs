// API/Controllers/AdminTestController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AgriculturePlatform.Application.Interfaces;
using AgriculturePlatform.API.Filters;
using System.Security.Claims;

namespace AgriculturePlatform.API.Controllers;

[ApiController]
[Route("api/admin/farms/{farmId}/test")]
[Authorize]
[AuthorizeFarm]
public class AdminTestController : ControllerBase
{
    private readonly IIoTSimulatorService _ioTSimulatorService;
    private readonly IAlertNotificationService _alertNotificationService;

    public AdminTestController(
        IIoTSimulatorService iotSimulatorService,
        IAlertNotificationService alertNotificationService)
    {
        _ioTSimulatorService = iotSimulatorService;
        _alertNotificationService = alertNotificationService;
    }

    private int GetCurrentFarmId() => int.Parse(User.FindFirst("farmId")?.Value ?? "0");
    private int GetCurrentAdminId() => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

    [HttpPost("generate-critical-alerts")]
    public async Task<IActionResult> GenerateCriticalAlerts()
    {
        var farmId = GetCurrentFarmId();
        var adminId = GetCurrentAdminId();
        await _ioTSimulatorService.GenerateTestCriticalAlertsAsync(farmId, adminId);
        return Ok(new { message = "Critical test alerts generated and emails sent" });
    }

    [HttpPost("generate-random-severity")]
    public async Task<IActionResult> GenerateRandomSeverityReadings()
    {
        var farmId = GetCurrentFarmId();
        var adminId = GetCurrentAdminId();
        await _ioTSimulatorService.GenerateRandomSeverityReadingsAsync(farmId, adminId);
        return Ok(new { message = "Random severity readings generated and emails sent" });
    }

    [HttpPost("send-test-email")]
    public async Task<IActionResult> SendTestEmail([FromQuery] string email)
    {
        await _alertNotificationService.SendTestAlertEmailAsync(email, "Test User");
        return Ok(new { message = $"Test email sent to {email}" });
    }
}