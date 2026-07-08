// AgriculturePlatform.API/Controllers/AdminSensorController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims; 
using AgriculturePlatform.Application.DTOs.Sensor;
using AgriculturePlatform.Application.Interfaces;
using AgriculturePlatform.API.Filters;

namespace AgriculturePlatform.API.Controllers;

[ApiController]
[Route("api/admin/farms/{farmId}/sensors")]
[Authorize]
[AuthorizeFarm]
public class AdminSensorController : ControllerBase
{
    private readonly ISensorReadingService _sensorService;
    private readonly IAlertService _alertService;

    public AdminSensorController(ISensorReadingService sensorService, IAlertService alertService)
    {
        _sensorService = sensorService;
        _alertService = alertService;
    }

    private int GetCurrentFarmId() => int.Parse(User.FindFirst("farmId")?.Value ?? "0");
    private int GetCurrentAdminId() => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

    // GET: api/admin/farms/{farmId}/sensors
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] SensorReadingFilterDto filter)
    {
        var farmId = GetCurrentFarmId();
        var result = await _sensorService.GetAllReadingsAsync(filter, farmId);
        return Ok(result);
    }

    // GET: api/admin/farms/{farmId}/sensors/latest
    [HttpGet("latest")]
    public async Task<IActionResult> GetLatestPerField()
    {
        var farmId = GetCurrentFarmId();
        var result = await _sensorService.GetLatestReadingsPerFieldAsync(farmId);
        return Ok(result);
    }

    // POST: api/admin/farms/{farmId}/sensors/manual
    [HttpPost("manual")]
    public async Task<IActionResult> AddManualReading([FromBody] CreateManualSensorReadingDto dto)
    {
        var farmId = GetCurrentFarmId();
        var adminId = GetCurrentAdminId();
        var result = await _sensorService.AddManualReadingAsync(dto, farmId, adminId);
        
        if (!result.Success)
            return BadRequest(result);
            
        return Ok(result);
    }

// GET: api/admin/farms/{farmId}/sensors/field/{fieldId}/history
// AgriculturePlatform.API/Controllers/AdminSensorController.cs

[HttpGet("field/{fieldId}/history")]
public async Task<IActionResult> GetFieldHistory(
    int fieldId, 
    [FromQuery] DateTime? fromDate = null, 
    [FromQuery] DateTime? toDate = null)
{
    var farmId = GetCurrentFarmId();
    
    // Default to last 30 days if not specified
    var endDate = toDate?.ToUniversalTime() ?? DateTime.UtcNow;
    var startDate = fromDate?.ToUniversalTime() ?? endDate.AddDays(-30);
    
    var result = await _sensorService.GetReadingsByDateRangeAsync(fieldId, farmId, startDate, endDate);
    return Ok(result);
}


    // GET: api/admin/farms/{farmId}/sensors/threshold-violations
    [HttpGet("threshold-violations")]
    public async Task<IActionResult> GetThresholdViolations([FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
    {
        var farmId = GetCurrentFarmId();
        var result = await _sensorService.GetThresholdViolationsAsync(farmId, fromDate, toDate);
        return Ok(result);
    }

    // GET: api/admin/farms/{farmId}/sensors/statistics
    [HttpGet("statistics")]
    public async Task<IActionResult> GetStatistics(
        [FromQuery] string groupBy = "day",
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        var farmId = GetCurrentFarmId();
        var result = await _sensorService.GetAverageReadingsAsync(farmId, groupBy, fromDate, toDate);
        return Ok(result);
    }

    // GET: api/admin/farms/{farmId}/sensors/export
    [HttpGet("export")]
    public async Task<IActionResult> ExportToExcel(
        [FromQuery] int? fieldId = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        var farmId = GetCurrentFarmId();
        var result = await _sensorService.ExportToExcelAsync(farmId, fieldId, fromDate, toDate);
        
        if (!result.Success)
            return BadRequest(result);
            
        return File(result.Data!, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
            $"sensor_readings_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
    }

    
}