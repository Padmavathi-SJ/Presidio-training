using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using AgriculturePlatform.Application.DTOs.Alert;
using AgriculturePlatform.Application.Interfaces;
using AgriculturePlatform.API.Filters;

namespace AgriculturePlatform.API.Controllers;

[ApiController]
[Route("api/admin/farms/{farmId}/sensors/thresholds")]
[Authorize]
[AuthorizeFarm]
public class AdminAlertThresholdController : ControllerBase
{
    private readonly IAlertThresholdService _thresholdService;

    public AdminAlertThresholdController(IAlertThresholdService thresholdService)
    {
        _thresholdService = thresholdService;
    }

    private int GetCurrentFarmId() => int.Parse(User.FindFirst("farmId")?.Value ?? "0");
    private int GetCurrentAdminId() => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

    // GET: api/admin/farms/{farmId}/sensors/thresholds
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var farmId = GetCurrentFarmId();
        var result = await _thresholdService.GetAllThresholdsAsync(farmId);
        return Ok(result);
    }

    // GET: api/admin/farms/{farmId}/sensors/thresholds/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var farmId = GetCurrentFarmId();
        var result = await _thresholdService.GetThresholdByIdAsync(id, farmId);
        return Ok(result);
    }

    // POST: api/admin/farms/{farmId}/sensors/thresholds
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAlertThresholdDto dto)
    {
        var farmId = GetCurrentFarmId();
        var adminId = GetCurrentAdminId();
        var result = await _thresholdService.CreateThresholdAsync(dto, farmId, adminId);
        
        if (!result.Success)
            return BadRequest(result);
            
        return CreatedAtAction(nameof(GetById), new { farmId = farmId, id = result.Data.Id }, result);
    }

    // PUT: api/admin/farms/{farmId}/sensors/thresholds/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateAlertThresholdDto dto)
    {
        var farmId = GetCurrentFarmId();
        var adminId = GetCurrentAdminId();
        var result = await _thresholdService.UpdateThresholdAsync(id, dto, farmId, adminId);
        
        if (!result.Success)
            return BadRequest(result);
            
        return Ok(result);
    }

    // DELETE: api/admin/farms/{farmId}/sensors/thresholds/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var farmId = GetCurrentFarmId();
        var adminId = GetCurrentAdminId();
        var result = await _thresholdService.DeleteThresholdAsync(id, farmId, adminId);
        
        if (!result.Success)
            return BadRequest(result);
            
        return Ok(result);
    }
}
