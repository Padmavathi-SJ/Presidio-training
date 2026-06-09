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

    // GET: api/worker/farms/{farmId}/sensors/latest
    [HttpGet("latest")]
    public async Task<IActionResult> GetLatestReadings()
    {
        var farmId = GetCurrentFarmId();
        var allReadings = await _sensorService.GetLatestReadingsPerFieldAsync(farmId);
        
        // Filter only fields worker has access to
        var filteredReadings = allReadings.Data?
            .Where(r => HasFieldAccess(r.FieldId).Result)
            .ToList();
        
        return Ok(filteredReadings);
    }

    // GET: api/worker/farms/{farmId}/sensors/field/{fieldId}/latest
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

    // GET: api/worker/farms/{farmId}/alerts/unresolved
    [HttpGet("alerts/unresolved")]
    public async Task<IActionResult> GetUnresolvedAlerts()
    {
        var farmId = GetCurrentFarmId();
        var allAlerts = await _alertService.GetAllAlertsAsync(new AlertFilterDto { IsResolved = false }, farmId);
        
        // Filter only alerts for fields worker has access to
        var filteredAlerts = allAlerts.Data?.Items
            .Where(a => HasFieldAccess(a.FieldId).Result)
            .ToList();
        
        return Ok(filteredAlerts);
    }
}