// AgriculturePlatform.API/Controllers/AdminAlertsController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using AgriculturePlatform.Application.DTOs.Alert;
using AgriculturePlatform.Application.Interfaces;
using AgriculturePlatform.API.Filters;

namespace AgriculturePlatform.API.Controllers;

[ApiController]
[Route("api/admin/farms/{farmId}/alerts")]
[Authorize]
[AuthorizeFarm]
public class AdminAlertsController : ControllerBase
{
    private readonly IAlertService _alertService;

    public AdminAlertsController(IAlertService alertService)
    {
        _alertService = alertService;
    }

    private int GetCurrentFarmId() => int.Parse(User.FindFirst("farmId")?.Value ?? "0");
    private int GetCurrentAdminId() => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

    // GET: api/admin/farms/{farmId}/alerts
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] AlertFilterDto filter)
    {
        var farmId = GetCurrentFarmId();
        var result = await _alertService.GetAllAlertsAsync(filter, farmId);
        return Ok(result);
    }

    // GET: api/admin/farms/{farmId}/alerts/dashboard
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        var farmId = GetCurrentFarmId();
        var result = await _alertService.GetDashboardAlertsAsync(farmId);
        return Ok(result);
    }

    // GET: api/admin/farms/{farmId}/alerts/{id:int}
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var farmId = GetCurrentFarmId();
        var result = await _alertService.GetAlertByIdAsync(id, farmId);
        
        if (!result.Success)
            return NotFound(result);
            
        return Ok(result);
    }

    // GET: api/admin/farms/{farmId}/alerts/critical
    [HttpGet("critical")]
    public async Task<IActionResult> GetCriticalAlerts()
    {
        var farmId = GetCurrentFarmId();
        var result = await _alertService.GetCriticalAlertsAsync(farmId);
        return Ok(result);
    }

    // GET: api/admin/farms/{farmId}/alerts/statistics
    [HttpGet("statistics")]
    public async Task<IActionResult> GetStatistics([FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
    {
        var farmId = GetCurrentFarmId();
        var result = await _alertService.GetAlertStatisticsAsync(farmId, fromDate, toDate);
        return Ok(result);
    }

    // GET: api/admin/farms/{farmId}/alerts/unresolved-count
    [HttpGet("unresolved-count")]
    public async Task<IActionResult> GetUnresolvedCount()
    {
        var farmId = GetCurrentFarmId();
        var result = await _alertService.GetUnresolvedCountAsync(farmId);
        return Ok(result);
    }

    // PUT: api/admin/farms/{farmId}/alerts/{id:int}/resolve
    [HttpPut("{id:int}/resolve")]
    public async Task<IActionResult> ResolveAlert(int id, [FromBody] ResolveAlertDto dto)
    {
        var farmId = GetCurrentFarmId();
        var adminId = GetCurrentAdminId();
        var result = await _alertService.ResolveAlertAsync(id, dto, farmId, adminId);
        
        if (!result.Success)
            return BadRequest(result);
            
        return Ok(result);
    }
}