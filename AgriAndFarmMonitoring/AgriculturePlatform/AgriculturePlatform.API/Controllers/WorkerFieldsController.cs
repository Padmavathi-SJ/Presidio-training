// AgriculturePlatform.API/Controllers/WorkerFieldsController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using AgriculturePlatform.Application.Interfaces;
using AgriculturePlatform.API.Filters;

namespace AgriculturePlatform.API.Controllers;

[ApiController]
[Route("api/worker/fields")]
[Authorize]
[AuthorizeWorkerFarm]
public class WorkerFieldsController : ControllerBase
{
    private readonly IWorkerFieldService _workerFieldService;

    public WorkerFieldsController(IWorkerFieldService workerFieldService)
    {
        _workerFieldService = workerFieldService;
    }

    private int GetCurrentWorkerId()
    {
        var workerIdClaim = User.FindFirst("workerId")?.Value 
                         ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(workerIdClaim, out var id) ? id : 0;
    }

    private int GetCurrentFarmId()
    {
        return int.Parse(User.FindFirst("farmId")?.Value ?? "0");
    }

    /// <summary>
    /// Get all fields assigned to the current worker
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetMyAssignedFields()
    {
        var workerId = GetCurrentWorkerId();
        var farmId = GetCurrentFarmId();
        
        var result = await _workerFieldService.GetMyAssignedFieldsAsync(workerId, farmId);
        
        if (!result.Success)
            return BadRequest(result);
            
        return Ok(result);
    }

    /// <summary>
    /// Get detailed information about a specific assigned field including crop cycles
    /// </summary>
    [HttpGet("{fieldId}")]
    public async Task<IActionResult> GetAssignedFieldDetail(int fieldId)
    {
        var workerId = GetCurrentWorkerId();
        var farmId = GetCurrentFarmId();
        
        var result = await _workerFieldService.GetAssignedFieldDetailAsync(fieldId, workerId, farmId);
        
        if (!result.Success)
            return NotFound(result);
            
        return Ok(result);
    }
}