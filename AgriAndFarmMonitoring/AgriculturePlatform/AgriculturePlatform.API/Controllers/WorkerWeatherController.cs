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

    public WorkerWeatherController(IWeatherService weatherService)
    {
        _weatherService = weatherService;
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

    // GET: api/worker/farms/{farmId}/weather/current/{fieldId}
[HttpGet("current/{fieldId}")]
public async Task<IActionResult> GetCurrentWeather(int fieldId)
{
    var farmId = GetCurrentFarmId();
    // Pass null for adminId since worker doesn't have one
    var result = await _weatherService.GetCurrentWeatherAsync(fieldId, farmId, null);
    return Ok(result);
}

    // GET: api/worker/farms/{farmId}/weather/forecast/{fieldId}
    [HttpGet("forecast/{fieldId}")]
    public async Task<IActionResult> GetForecast(int fieldId)
    {
        var farmId = GetCurrentFarmId();
        var result = await _weatherService.GetForecastAsync(fieldId, farmId);
        return Ok(result);
    }

    // GET: api/worker/farms/{farmId}/weather/history
    [HttpGet("history")]
    public async Task<IActionResult> GetHistory([FromQuery] WeatherHistoryFilterDto filter)
    {
        var farmId = GetCurrentFarmId();
        var result = await _weatherService.GetWeatherHistoryAsync(filter, farmId);
        return Ok(result);
    }

    // GET: api/worker/farms/{farmId}/weather/alerts
    [HttpGet("alerts")]
    public async Task<IActionResult> GetAlerts()
    {
        var farmId = GetCurrentFarmId();
        var result = await _weatherService.GetActiveWeatherAlertsAsync(farmId);
        return Ok(result);
    }
}