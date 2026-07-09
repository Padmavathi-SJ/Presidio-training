// AgriculturePlatform.API/Controllers/WorkerWeatherController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using AgriculturePlatform.Application.DTOs.Weather;
using AgriculturePlatform.Application.Interfaces;
using AgriculturePlatform.API.Filters;

namespace AgriculturePlatform.API.Controllers;

[ApiController]
[Route("api/worker/farms/{farmId}/weather")]
[Authorize]
[AuthorizeWorkerFarm]
public class WorkerWeatherController : ControllerBase
{
    private readonly IWeatherService _weatherService;
    private readonly IWorkerFieldAssignmentRepository _assignmentRepository;

    public WorkerWeatherController(IWeatherService weatherService, IWorkerFieldAssignmentRepository assignmentRepository)
    {
        _weatherService = weatherService;
        _assignmentRepository = assignmentRepository;
    }

    private int GetCurrentFarmId()
    {
        return int.Parse(User.FindFirst("farmId")?.Value ?? "0");
    }

    private int GetCurrentWorkerId()
    {
        var workerIdClaim = User.FindFirst("workerId")?.Value 
                         ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(workerIdClaim, out var id) ? id : 0;
    }

    private async Task<bool> HasFieldAccess(int fieldId)
    {
        return await _assignmentRepository.HasWorkerAccessToFieldAsync(GetCurrentWorkerId(), fieldId, GetCurrentFarmId());
    }

    private async Task<List<int>> GetAllowedFieldIdsAsync()
    {
        var fields = await _assignmentRepository.GetFieldsByWorkerAsync(GetCurrentWorkerId(), GetCurrentFarmId());
        return fields.Select(f => f.Id).ToList();
    }

    // GET: api/worker/farms/{farmId}/weather/current/{fieldId}
    [HttpGet("current/{fieldId}")]
    public async Task<IActionResult> GetCurrentWeather(int fieldId)
    {
        if (!await HasFieldAccess(fieldId))
            return Forbid();

        var farmId = GetCurrentFarmId();
        var result = await _weatherService.GetCurrentWeatherAsync(fieldId, farmId, null);
        return Ok(result);
    }

    // GET: api/worker/farms/{farmId}/weather/forecast/{fieldId}
    [HttpGet("forecast/{fieldId}")]
    public async Task<IActionResult> GetForecast(int fieldId)
    {
        if (!await HasFieldAccess(fieldId))
            return Forbid();

        var farmId = GetCurrentFarmId();
        var result = await _weatherService.GetForecastAsync(fieldId, farmId);
        return Ok(result);
    }

    // GET: api/worker/farms/{farmId}/weather/history
    [HttpGet("history")]
    public async Task<IActionResult> GetHistory([FromQuery] WeatherHistoryFilterDto filter)
    {
        var allowedFieldIds = await GetAllowedFieldIdsAsync();
        if (!allowedFieldIds.Any()) return Ok(new { data = new { items = new List<object>(), totalCount = 0 } });

        if (filter.FieldId.HasValue && !allowedFieldIds.Contains(filter.FieldId.Value))
            return Forbid();

        filter.AllowedFieldIds = allowedFieldIds;
        var farmId = GetCurrentFarmId();
        var result = await _weatherService.GetWeatherHistoryAsync(filter, farmId);
        return Ok(result);
    }

    // GET: api/worker/farms/{farmId}/weather/alerts
    [HttpGet("alerts")]
    public async Task<IActionResult> GetAlerts([FromQuery] WeatherAlertFilterDto filter)
    {
        var allowedFieldIds = await GetAllowedFieldIdsAsync();
        if (!allowedFieldIds.Any()) return Ok(new { data = new { items = new List<object>(), totalCount = 0 } });

        if (filter.FieldId.HasValue && !allowedFieldIds.Contains(filter.FieldId.Value))
            return Forbid();

        filter.AllowedFieldIds = allowedFieldIds;
        filter.IsActive = true;
        var farmId = GetCurrentFarmId();
        var result = await _weatherService.GetWeatherAlertsAsync(filter, farmId);
        return Ok(result);
    }

    // PUT: api/worker/farms/{farmId}/weather/alerts/{id}/resolve
    [HttpPut("alerts/{id}/resolve")]
    public async Task<IActionResult> ResolveAlert(int id, [FromBody] ResolveWeatherAlertDto dto)
    {
        var farmId = GetCurrentFarmId();
        var workerId = GetCurrentWorkerId();

        // Optional: verify access to field before resolving
        var alertResult = await _weatherService.GetWeatherAlertByIdAsync(id, farmId);
        if (!alertResult.Success)
        {
            return NotFound(alertResult);
        }

        if (!await HasFieldAccess(alertResult.Data.FieldId))
        {
            return Forbid();
        }

        var result = await _weatherService.ResolveWeatherAlertAsync(id, dto, farmId, workerId);
        return Ok(result);
    }
}