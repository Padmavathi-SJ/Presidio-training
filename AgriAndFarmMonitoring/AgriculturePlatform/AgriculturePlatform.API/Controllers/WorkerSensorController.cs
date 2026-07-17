// AgriculturePlatform.API/Controllers/WorkerSensorController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AgriculturePlatform.Application.DTOs.Sensor;
using AgriculturePlatform.Application.DTOs.Alert;
using AgriculturePlatform.Application.Interfaces;
using AgriculturePlatform.API.Filters;

namespace AgriculturePlatform.API.Controllers;

[ApiController]
[Route("api/worker/farms/{farmId}/sensors")]
[Authorize]
[AuthorizeWorkerFarm]
public class WorkerSensorController : ControllerBase
{
    private readonly ISensorReadingService _sensorService;
    private readonly IAlertService _alertService;
    private readonly IWorkerFieldAssignmentRepository _assignmentRepository;

    public WorkerSensorController(
        ISensorReadingService sensorService,
        IAlertService alertService,
        IWorkerFieldAssignmentRepository assignmentRepository)
    {
        _sensorService = sensorService;
        _alertService = alertService;
        _assignmentRepository = assignmentRepository;
    }

    private int GetCurrentFarmId() => int.Parse(User.FindFirst("farmId")?.Value ?? "0");
    private int GetCurrentWorkerId() => int.Parse(User.FindFirst("workerId")?.Value ?? "0");

    private async Task<bool> HasFieldAccess(int fieldId)
    {
        return await _assignmentRepository.HasWorkerAccessToFieldAsync(GetCurrentWorkerId(), fieldId, GetCurrentFarmId());
    }

    private async Task<List<int>> GetAllowedFieldIdsAsync()
    {
        var fields = await _assignmentRepository.GetFieldsByWorkerAsync(GetCurrentWorkerId(), GetCurrentFarmId());
        return fields.Select(f => f.Id).ToList();
    }

    [HttpGet]
    public async Task<IActionResult> GetAllReadings([FromQuery] SensorReadingFilterDto filter)
    {
        var allowedFieldIds = await GetAllowedFieldIdsAsync();
        if (!allowedFieldIds.Any()) return Ok(new { data = new { items = new List<object>(), totalCount = 0 } });
        
        if (filter.FieldId.HasValue && !allowedFieldIds.Contains(filter.FieldId.Value))
            return Forbid();

        filter.AllowedFieldIds = allowedFieldIds;
        var farmId = GetCurrentFarmId();
        var result = await _sensorService.GetAllReadingsAsync(filter, farmId);
        return Ok(result);
    }

    [HttpGet("latest")]
    public async Task<IActionResult> GetLatestReadings()
    {
        var farmId = GetCurrentFarmId();
        var allowedFieldIds = await GetAllowedFieldIdsAsync();
        if (!allowedFieldIds.Any()) return Ok(new List<object>());

        var result = await _sensorService.GetLatestReadingsPerFieldAsync(farmId, allowedFieldIds);
        return Ok(result);
    }

    [HttpGet("field/{fieldId}/latest")]
    public async Task<IActionResult> GetFieldLatestReadings(int fieldId)
    {
        if (!await HasFieldAccess(fieldId))
            return Forbid();

        var farmId = GetCurrentFarmId();
        var filter = new SensorReadingFilterDto { FieldId = fieldId, LatestOnly = true };
        var result = await _sensorService.GetAllReadingsAsync(filter, farmId);
        return Ok(result);
    }

    [HttpGet("alerts")]
    public async Task<IActionResult> GetAlerts([FromQuery] AlertFilterDto filter)
    {
        var allowedFieldIds = await GetAllowedFieldIdsAsync();
        if (!allowedFieldIds.Any()) return Ok(new { data = new { items = new List<object>(), totalCount = 0 } });

        if (filter.FieldId.HasValue && !allowedFieldIds.Contains(filter.FieldId.Value))
            return Forbid();

        filter.AllowedFieldIds = allowedFieldIds;
        var farmId = GetCurrentFarmId();
        var result = await _alertService.GetAllAlertsAsync(filter, farmId);
        return Ok(result);
    }

    [HttpGet("alerts/unresolved")]
    public async Task<IActionResult> GetUnresolvedAlerts()
    {
        var farmId = GetCurrentFarmId();
        var allowedFieldIds = await GetAllowedFieldIdsAsync();
        if (!allowedFieldIds.Any()) return Ok(new List<object>());

        var filter = new AlertFilterDto { IsResolved = false, AllowedFieldIds = allowedFieldIds };
        var result = await _alertService.GetAllAlertsAsync(filter, farmId);
        return Ok(result.Data?.Items ?? new List<AlertDto>());
    }

    [HttpPut("alerts/{id}/resolve")]
    public async Task<IActionResult> ResolveAlert(int id, [FromBody] ResolveAlertDto dto)
    {
        var farmId = GetCurrentFarmId();
        var workerId = GetCurrentWorkerId();
        
        // Ensure worker has access to the alert's field
        var alertResult = await _alertService.GetAlertByIdAsync(id, farmId);
        if (!alertResult.Success || alertResult.Data == null)
            return NotFound(alertResult);

        var allowedFieldIds = await GetAllowedFieldIdsAsync();
        if (!allowedFieldIds.Contains(alertResult.Data.FieldId))
            return Forbid();

        var result = await _alertService.ResolveAlertAsync(id, dto, farmId, null, workerId);
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
    
    [HttpGet("statistics")]
    public async Task<IActionResult> GetStatistics([FromQuery] string groupBy = "day", [FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
    {
        var allowedFieldIds = await GetAllowedFieldIdsAsync();
        if (!allowedFieldIds.Any()) return Ok(new { data = new { } });

        var farmId = GetCurrentFarmId();
        var result = await _sensorService.GetAverageReadingsAsync(farmId, groupBy, fromDate, toDate, allowedFieldIds);
        return Ok(result);
    }
    
    [HttpGet("thresholds")]
    public async Task<IActionResult> GetThresholds([FromServices] IAlertThresholdService thresholdService)
    {
        var farmId = GetCurrentFarmId();
        var result = await thresholdService.GetAllThresholdsAsync(farmId);
        return Ok(result);
    }
}