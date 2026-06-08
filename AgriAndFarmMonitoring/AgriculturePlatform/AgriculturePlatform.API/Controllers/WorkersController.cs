// AgriculturePlatform.API/Controllers/WorkersController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using AgriculturePlatform.Application.Common;
using AgriculturePlatform.Application.DTOs.Worker;
using AgriculturePlatform.Application.Interfaces;
using AgriculturePlatform.API.Filters;

namespace AgriculturePlatform.API.Controllers;

[ApiController]
[Route("api/farms/{farmId}/workers")]
[Authorize]
[AuthorizeFarm]
public class WorkersController : ControllerBase
{
    private readonly IWorkerService _workerService;

    public WorkersController(IWorkerService workerService)
    {
        _workerService = workerService;
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

    // GET: api/farms/{farmId}/workers
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] WorkerFilterDto filter)
    {
        var farmId = GetCurrentFarmId();
        var result = await _workerService.GetAllAsync(filter, farmId);
        return Ok(result);
    }

    // GET: api/farms/{farmId}/workers/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var farmId = GetCurrentFarmId();
        var result = await _workerService.GetByIdAsync(id, farmId);
        
        if (!result.Success)
            return NotFound(result);
            
        return Ok(result);
    }

    // GET: api/farms/{farmId}/workers/{id}/login-history
    [HttpGet("{id}/login-history")]
    public async Task<IActionResult> GetLoginHistory(int id)
    {
        var farmId = GetCurrentFarmId();
        var result = await _workerService.GetLoginHistoryAsync(id, farmId);
        
        if (!result.Success)
            return NotFound(result);
            
        return Ok(result);
    }

    // POST: api/farms/{farmId}/workers
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateWorkerDto dto)
    {
        var farmId = GetCurrentFarmId();
        var adminId = GetCurrentAdminId();
        var ipAddress = GetIpAddress();
        var userAgent = GetUserAgent();
        
        var result = await _workerService.CreateAsync(dto, farmId, adminId, ipAddress, userAgent);
        
        if (!result.Success)
            return BadRequest(result);
            
        return CreatedAtAction(nameof(GetById), new { farmId, id = result.Data?.Id }, result);
    }

    // PUT: api/farms/{farmId}/workers/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateWorkerDto dto)
    {
        var farmId = GetCurrentFarmId();
        var adminId = GetCurrentAdminId();
        var ipAddress = GetIpAddress();
        var userAgent = GetUserAgent();
        
        var result = await _workerService.UpdateAsync(id, dto, farmId, adminId, ipAddress, userAgent);
        
        if (!result.Success)
            return NotFound(result);
            
        return Ok(result);
    }

    // PUT: api/farms/{farmId}/workers/{id}/activate
    [HttpPut("{id}/activate")]
    public async Task<IActionResult> Activate(int id)
    {
        var farmId = GetCurrentFarmId();
        var adminId = GetCurrentAdminId();
        var ipAddress = GetIpAddress();
        var userAgent = GetUserAgent();
        
        var result = await _workerService.ActivateAsync(id, farmId, adminId, ipAddress, userAgent);
        
        if (!result.Success)
            return BadRequest(result);
            
        return Ok(result);
    }

    // PUT: api/farms/{farmId}/workers/{id}/deactivate
    [HttpPut("{id}/deactivate")]
    public async Task<IActionResult> Deactivate(int id)
    {
        var farmId = GetCurrentFarmId();
        var adminId = GetCurrentAdminId();
        var ipAddress = GetIpAddress();
        var userAgent = GetUserAgent();
        
        var result = await _workerService.DeactivateAsync(id, farmId, adminId, ipAddress, userAgent);
        
        if (!result.Success)
            return BadRequest(result);
            
        return Ok(result);
    }

    // PUT: api/farms/{farmId}/workers/{id}/reset-password
    [HttpPut("{id}/reset-password")]
    public async Task<IActionResult> ResetPassword(int id, [FromBody] ChangeWorkerPasswordDto dto)
    {
        var farmId = GetCurrentFarmId();
        var adminId = GetCurrentAdminId();
        var ipAddress = GetIpAddress();
        var userAgent = GetUserAgent();
        
        var result = await _workerService.ResetPasswordAsync(id, farmId, adminId, dto.NewPassword, ipAddress, userAgent);
        
        if (!result.Success)
            return BadRequest(result);
            
        return Ok(result);
    }

    // DELETE: api/farms/{farmId}/workers/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> SoftDelete(int id)
    {
        var farmId = GetCurrentFarmId();
        var adminId = GetCurrentAdminId();
        var ipAddress = GetIpAddress();
        var userAgent = GetUserAgent();
        
        var result = await _workerService.SoftDeleteAsync(id, farmId, adminId, ipAddress, userAgent);
        
        if (!result.Success)
            return BadRequest(result);
            
        return Ok(result);
    }
}