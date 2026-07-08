// AgriculturePlatform.API/Controllers/CropCyclesController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using AgriculturePlatform.Application.Common;
using AgriculturePlatform.Application.DTOs.CropCycle;
using AgriculturePlatform.Application.Interfaces;
using AgriculturePlatform.API.Filters;

namespace AgriculturePlatform.API.Controllers;

[ApiController]
[Route("api/farms/{farmId}/crop-cycles")]
[Authorize]
[AuthorizeFarm]
public class CropCyclesController : ControllerBase
{
    private readonly ICropCycleService _cropCycleService;

    public CropCyclesController(ICropCycleService cropCycleService)
    {
        _cropCycleService = cropCycleService;
    }

    private int GetCurrentFarmId()
    {
        return int.Parse(User.FindFirst("farmId")?.Value ?? "0");
    }

    private int GetCurrentAdminId()
    {
        var adminIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                        ?? User.FindFirst("sub")?.Value 
                        ?? User.FindFirst("id")?.Value;
        return int.TryParse(adminIdClaim, out var id) ? id : 0;
    }

    private string GetIpAddress()
    {
        return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
    }

    private string GetUserAgent()
    {
        return HttpContext.Request.Headers["User-Agent"].ToString() ?? "Unknown";
    }

    // GET: api/farms/{farmId}/crop-cycles
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] CropCycleFilterDto filter)
    {
        var farmId = GetCurrentFarmId();
        var result = await _cropCycleService.GetAllAsync(filter, farmId);
        return Ok(result);
    }

    // GET: api/farms/{farmId}/crop-cycles/overdue
    [HttpGet("overdue")]
    public async Task<IActionResult> GetOverdue()
    {
        var farmId = GetCurrentFarmId();
        var result = await _cropCycleService.GetOverdueAsync(farmId);
        return Ok(result);
    }

    // GET: api/farms/{farmId}/crop-cycles/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var farmId = GetCurrentFarmId();
        var result = await _cropCycleService.GetByIdAsync(id, farmId);
        
        if (!result.Success)
            return NotFound(result);
            
        return Ok(result);
    }

    // POST: api/farms/{farmId}/crop-cycles
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCropCycleDto dto)
    {
        var farmId = GetCurrentFarmId();
        var adminId = GetCurrentAdminId();
        var ipAddress = GetIpAddress();
        var userAgent = GetUserAgent();
        
        var result = await _cropCycleService.CreateAsync(dto, farmId, adminId, ipAddress, userAgent);
        
        if (!result.Success)
            return BadRequest(result);
            
        return CreatedAtAction(nameof(GetById), new { farmId, id = result.Data?.Id }, result);
    }

    // PUT: api/farms/{farmId}/crop-cycles/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCropCycleDto dto)
    {
        var farmId = GetCurrentFarmId();
        var adminId = GetCurrentAdminId();
        var ipAddress = GetIpAddress();
        var userAgent = GetUserAgent();
        
        var result = await _cropCycleService.UpdateAsync(id, dto, farmId, adminId, ipAddress, userAgent);
        
        if (!result.Success)
            return NotFound(result);
            
        return Ok(result);
    }

    // DELETE: api/farms/{farmId}/crop-cycles/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> SoftDelete(int id)
    {
        var farmId = GetCurrentFarmId();
        var adminId = GetCurrentAdminId();
        var ipAddress = GetIpAddress();
        var userAgent = GetUserAgent();
        
        var result = await _cropCycleService.SoftDeleteAsync(id, farmId, adminId, ipAddress, userAgent);
        
        if (!result.Success)
            return BadRequest(result);
            
        return Ok(result);
    }

    // In CropCyclesController.cs - Add this endpoint

// POST: api/farms/{farmId}/crop-cycles/{id}/update-growth-stage
[HttpPost("{id}/update-growth-stage")]
public async Task<IActionResult> UpdateGrowthStage(int id)
{
    var farmId = GetCurrentFarmId();
    var adminId = GetCurrentAdminId();
    var ipAddress = GetIpAddress();
    var userAgent = GetUserAgent();
    
    var result = await _cropCycleService.UpdateGrowthStageManuallyAsync(id, farmId, adminId, ipAddress, userAgent);
    
    if (!result.Success)
        return BadRequest(result);
        
    return Ok(result);
}
}