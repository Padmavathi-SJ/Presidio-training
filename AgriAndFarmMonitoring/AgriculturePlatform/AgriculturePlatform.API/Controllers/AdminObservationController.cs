// AgriculturePlatform.API/Controllers/AdminObservationController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using AgriculturePlatform.Application.DTOs.Observation;
using AgriculturePlatform.Application.Interfaces;
using AgriculturePlatform.API.Filters;

namespace AgriculturePlatform.API.Controllers;

[ApiController]
[Route("api/admin/farms/{farmId}/observations")]
[Authorize]
[AuthorizeFarm]
public class AdminObservationController : ControllerBase
{
    private readonly IObservationService _observationService;

    public AdminObservationController(IObservationService observationService)
    {
        _observationService = observationService;
    }

    private int GetCurrentFarmId() => int.Parse(User.FindFirst("farmId")?.Value ?? "0");
    private int GetCurrentAdminId() => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

    // GET: api/admin/farms/{farmId}/observations
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] ObservationFilterDto filter)
    {
        var farmId = GetCurrentFarmId();
        var result = await _observationService.GetAllObservationsAsync(filter, farmId);
        return Ok(result);
    }

    // GET: api/admin/farms/{farmId}/observations/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var farmId = GetCurrentFarmId();
        var result = await _observationService.GetObservationByIdAsync(id, farmId);
        
        if (!result.Success)
            return NotFound(result);
            
        return Ok(result);
    }

    // GET: api/admin/farms/{farmId}/observations/field/{fieldId}
    [HttpGet("field/{fieldId}")]
    public async Task<IActionResult> GetByField(int fieldId)
    {
        var farmId = GetCurrentFarmId();
        var result = await _observationService.GetObservationsByFieldAsync(fieldId, farmId);
        return Ok(result);
    }

    // GET: api/admin/farms/{farmId}/observations/crop-cycle/{cropCycleId}
    [HttpGet("crop-cycle/{cropCycleId}")]
    public async Task<IActionResult> GetByCropCycle(int cropCycleId)
    {
        var farmId = GetCurrentFarmId();
        var result = await _observationService.GetObservationsByCropCycleAsync(cropCycleId, farmId);
        return Ok(result);
    }

    // GET: api/admin/farms/{farmId}/observations/worker/{workerId}
    [HttpGet("worker/{workerId}")]
    public async Task<IActionResult> GetByWorker(int workerId)
    {
        var farmId = GetCurrentFarmId();
        var result = await _observationService.GetObservationsByWorkerAsync(workerId, farmId);
        return Ok(result);
    }

    // GET: api/admin/farms/{farmId}/observations/date-range
    [HttpGet("date-range")]
    public async Task<IActionResult> GetByDateRange([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate)
    {
        var farmId = GetCurrentFarmId();
        var result = await _observationService.GetObservationsByDateRangeAsync(farmId, fromDate, toDate);
        return Ok(result);
    }

    // GET: api/admin/farms/{farmId}/observations/statistics/pest
    [HttpGet("statistics/pest")]
    public async Task<IActionResult> GetPestStatistics([FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
    {
        var farmId = GetCurrentFarmId();
        var result = await _observationService.GetPestStatisticsAsync(farmId, fromDate, toDate);
        return Ok(result);
    }

    // PUT: api/admin/farms/{farmId}/observations/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateObservationDto dto)
    {
        var farmId = GetCurrentFarmId();
        var adminId = GetCurrentAdminId();
        var result = await _observationService.UpdateObservationAsync(id, dto, farmId, adminId);
        
        if (!result.Success)
            return NotFound(result);
            
        return Ok(result);
    }

    // DELETE: api/admin/farms/{farmId}/observations/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var farmId = GetCurrentFarmId();
        var adminId = GetCurrentAdminId();
        var result = await _observationService.DeleteObservationAsync(id, farmId, adminId);
        
        if (!result.Success)
            return BadRequest(result);
            
        return Ok(result);
    }
}