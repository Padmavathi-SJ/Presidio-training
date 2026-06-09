// AgriculturePlatform.API/Controllers/AdminAlertController.cs
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
public class AdminAlertController : ControllerBase
{
    private readonly IAlertService _alertService;

    public AdminAlertController(IAlertService alertService)
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

    // GET: api/admin/farms/{farmId}/alerts/unresolved
    [HttpGet("unresolved")]
    public async Task<IActionResult> GetUnresolved([FromQuery] AlertFilterDto filter)
    {
        filter.IsResolved = false;
        var farmId = GetCurrentFarmId();
        var result = await _alertService.GetAllAlertsAsync(filter, farmId);
        return Ok(result);
    }

    // GET: api/admin/farms/{farmId}/alerts/statistics
    [HttpGet("statistics")]
    public async Task<IActionResult> GetStatistics([FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
    {
        var farmId = GetCurrentFarmId();
        var result = await _alertService.GetStatisticsAsync(farmId, fromDate, toDate);
        return Ok(result);
    }

    // PUT: api/admin/farms/{farmId}/alerts/{id}/resolve
    [HttpPut("{id}/resolve")]
    public async Task<IActionResult> ResolveAlert(int id, [FromBody] ResolveAlertDto dto)
    {
        var farmId = GetCurrentFarmId();
        var adminId = GetCurrentAdminId();
        var result = await _alertService.ResolveAlertAsync(id, dto, farmId, adminId);
        
        if (!result.Success)
            return NotFound(result);
            
        return Ok(result);
    }
}