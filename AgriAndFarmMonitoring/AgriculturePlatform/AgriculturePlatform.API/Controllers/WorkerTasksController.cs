// AgriculturePlatform.API/Controllers/WorkerTasksController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using AgriculturePlatform.Application.DTOs.WorkerTask;
using AgriculturePlatform.Application.Interfaces;
using AgriculturePlatform.API.Filters;

namespace AgriculturePlatform.API.Controllers;

[ApiController]
[Route("api/worker/tasks")]
[Authorize]
[AuthorizeWorkerFarm]
public class WorkerTasksController : ControllerBase
{
    private readonly IWorkerTaskService _workerTaskService;

    public WorkerTasksController(IWorkerTaskService workerTaskService)
    {
        _workerTaskService = workerTaskService;
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
    /// Get all tasks assigned to the current worker
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetMyTasks([FromQuery] WorkerTaskFilterDto filter)
    {
        var workerId = GetCurrentWorkerId();
        var farmId = GetCurrentFarmId();
        
        var result = await _workerTaskService.GetMyTasksAsync(filter, workerId, farmId);
        return Ok(result);
    }

    /// <summary>
    /// Get task statistics for the worker dashboard
    /// </summary>
    [HttpGet("statistics")]
    public async Task<IActionResult> GetStatistics()
    {
        var workerId = GetCurrentWorkerId();
        var farmId = GetCurrentFarmId();
        
        var result = await _workerTaskService.GetTaskStatisticsAsync(workerId, farmId);
        return Ok(result);
    }

    /// <summary>
    /// Get task history (completed tasks)
    /// </summary>
    [HttpGet("history")]
    public async Task<IActionResult> GetTaskHistory([FromQuery] WorkerTaskFilterDto filter)
    {
        var workerId = GetCurrentWorkerId();
        var farmId = GetCurrentFarmId();
        
        var result = await _workerTaskService.GetTaskHistoryAsync(filter, workerId, farmId);
        return Ok(result);
    }

    /// <summary>
    /// Get a specific task by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetTaskById(int id)
    {
        var workerId = GetCurrentWorkerId();
        var farmId = GetCurrentFarmId();
        
        var result = await _workerTaskService.GetTaskByIdAsync(id, workerId, farmId);
        
        if (!result.Success)
            return NotFound(result);
            
        return Ok(result);
    }

    /// <summary>
    /// Update task status (PENDING -> IN_PROGRESS -> COMPLETED)
    /// </summary>
    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateTaskStatus(int id, [FromBody] UpdateWorkerTaskStatusDto dto)
    {
        var workerId = GetCurrentWorkerId();
        var farmId = GetCurrentFarmId();
        
        var result = await _workerTaskService.UpdateTaskStatusAsync(id, dto, workerId, farmId);
        
        if (!result.Success)
            return BadRequest(result);
            
        return Ok(result);
    }
}