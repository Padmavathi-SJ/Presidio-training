// AgriculturePlatform.API/Controllers/AdminWeatherController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using AgriculturePlatform.Application.DTOs.Weather;
using AgriculturePlatform.Application.Interfaces;
using AgriculturePlatform.API.Filters;

namespace AgriculturePlatform.API.Controllers;

[ApiController]
[Route("api/admin/farms/{farmId}/weather")]
[Authorize]
[AuthorizeFarm]
public class AdminWeatherController : ControllerBase
{
    private readonly IWeatherService _weatherService;
    private readonly IFieldRepository _fieldRepository; 
    public AdminWeatherController(IWeatherService weatherService, IFieldRepository fieldRepository)
    {
        _weatherService = weatherService;
         _fieldRepository = fieldRepository; 
    }

    private int GetCurrentFarmId() => int.Parse(User.FindFirst("farmId")?.Value ?? "0");
    private int GetCurrentAdminId() => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

// GET: api/admin/farms/{farmId}/weather/current/{fieldId}
[HttpGet("current/{fieldId}")]
public async Task<IActionResult> GetCurrentWeather(int fieldId)
{
    var farmId = GetCurrentFarmId();
    var adminId = GetCurrentAdminId();
    // Pass adminId for admin users
    var result = await _weatherService.GetCurrentWeatherAsync(fieldId, farmId, adminId);
    return Ok(result);
}


    // GET: api/admin/farms/{farmId}/weather/forecast/{fieldId}
    [HttpGet("forecast/{fieldId}")]
    public async Task<IActionResult> GetForecast(int fieldId)
    {
        var farmId = GetCurrentFarmId();
        var result = await _weatherService.GetForecastAsync(fieldId, farmId);
        return Ok(result);
    }

    // GET: api/admin/farms/{farmId}/weather/history
    [HttpGet("history")]
    public async Task<IActionResult> GetHistory([FromQuery] WeatherHistoryFilterDto filter)
    {
        var farmId = GetCurrentFarmId();
        var result = await _weatherService.GetWeatherHistoryAsync(filter, farmId);
        return Ok(result);
    }

    // GET: api/admin/farms/{farmId}/weather/alerts
    [HttpGet("alerts")]
    public async Task<IActionResult> GetAlerts()
    {
        var farmId = GetCurrentFarmId();
        var result = await _weatherService.GetActiveWeatherAlertsAsync(farmId);
        return Ok(result);
    }

    // POST: api/admin/farms/{farmId}/weather/manual
    [HttpPost("manual")]
    public async Task<IActionResult> AddManualEntry([FromBody] ManualWeatherEntryDto dto)
    {
        var farmId = GetCurrentFarmId();
        var adminId = GetCurrentAdminId();
        var result = await _weatherService.AddManualWeatherEntryAsync(dto, farmId, adminId);
        return Ok(result);
    }

    // PUT: api/admin/farms/{farmId}/weather/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateWeather(int id, [FromBody] ManualWeatherEntryDto dto)
    {
        var farmId = GetCurrentFarmId();
        var adminId = GetCurrentAdminId();
        var result = await _weatherService.UpdateWeatherDataAsync(id, dto, farmId, adminId);
        return Ok(result);
    }

    // DELETE: api/admin/farms/{farmId}/weather/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteWeather(int id)
    {
        var farmId = GetCurrentFarmId();
        var adminId = GetCurrentAdminId();
        var result = await _weatherService.DeleteWeatherDataAsync(id, farmId, adminId);
        return Ok(result);
    }

// POST: api/admin/farms/{farmId}/weather/refresh/{fieldId}
[HttpPost("refresh/{fieldId}")]
public async Task<IActionResult> RefreshWeather(int fieldId)
{
    var farmId = GetCurrentFarmId();
    var adminId = GetCurrentAdminId();  // ← Get adminId
    var result = await _weatherService.RefreshWeatherDataAsync(fieldId, farmId, adminId);  // ← Pass adminId
    return Ok(result);
}

    // POST: api/admin/farms/{farmId}/weather/refresh-all
[HttpPost("refresh-all")]
public async Task<IActionResult> RefreshAllWeather()
{
    var farmId = GetCurrentFarmId();
    var adminId = GetCurrentAdminId();  // ← Get adminId
    var result = await _weatherService.RefreshAllFieldsWeatherAsync(farmId, adminId);  // ← Pass adminId
    return Ok(result);
}

    // GET: api/admin/farms/{farmId}/weather/settings
    [HttpGet("settings")]
    public async Task<IActionResult> GetSettings()
    {
        var farmId = GetCurrentFarmId();
        var result = await _weatherService.GetApiSettingsAsync(farmId);
        return Ok(result);
    }

    // PUT: api/admin/farms/{farmId}/weather/settings
    [HttpPut("settings")]
    public async Task<IActionResult> UpdateSettings([FromBody] WeatherApiSettingsDto dto)
    {
        var farmId = GetCurrentFarmId();
        var result = await _weatherService.UpdateApiSettingsAsync(dto, farmId);
        return Ok(result);
    }

    // Add to AdminWeatherController.cs
[HttpGet("debug-fields")]
public async Task<IActionResult> DebugFields()
{
    var farmId = GetCurrentFarmId();
    var fields = await _fieldRepository.GetAllAsync(farmId);
    
    var result = fields.Select(f => new
    {
        f.Id,
        f.FieldName,
        f.Latitude,
        f.Longitude,
        HasCoordinates = f.Latitude.HasValue && f.Longitude.HasValue,
        Location = f.Location
    });
    
    return Ok(result);
}
}