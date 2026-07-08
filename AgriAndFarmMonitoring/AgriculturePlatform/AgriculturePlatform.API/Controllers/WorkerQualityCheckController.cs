// API/Controllers/WorkerQualityCheckController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using AgriculturePlatform.Application.Common;
using AgriculturePlatform.Application.DTOs.QualityCheck;
using AgriculturePlatform.Application.Interfaces;
using AgriculturePlatform.API.Filters;

namespace AgriculturePlatform.API.Controllers;

[ApiController]
[Route("api/worker/quality-checks")]
[Authorize]
[AuthorizeWorkerFarm]
public class WorkerQualityCheckController : ControllerBase
{
    private readonly IQualityCheckService _qualityCheckService;
    private readonly IHarvestRepository _harvestRepository;
    private readonly IWorkerRepository _workerRepository;

    public WorkerQualityCheckController(
        IQualityCheckService qualityCheckService,
        IHarvestRepository harvestRepository,
        IWorkerRepository workerRepository)
    {
        _qualityCheckService = qualityCheckService;
        _harvestRepository = harvestRepository;
        _workerRepository = workerRepository;
    }

    private int GetCurrentFarmId() => int.Parse(User.FindFirst("farmId")?.Value ?? "0");
    private int GetCurrentWorkerId() => int.Parse(User.FindFirst("workerId")?.Value ?? "0");

    private async Task<bool> HasHarvestAccess(int harvestId)
    {
        var harvest = await _harvestRepository.GetByIdAsync(harvestId, GetCurrentFarmId());
        return harvest != null;
    }

    [HttpGet("my")]
    public async Task<IActionResult> GetMyQualityChecks([FromQuery] QualityCheckFilterDto filter)
    {
        var farmId = GetCurrentFarmId();
        var workerId = GetCurrentWorkerId();
        filter.WorkerId = workerId;
        var result = await _qualityCheckService.GetAllQualityChecksAsync(filter, farmId);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var farmId = GetCurrentFarmId();
        var result = await _qualityCheckService.GetQualityCheckByIdAsync(id, farmId);
        if (!result.Success)
            return NotFound(result);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateQualityCheckDto dto)
    {
        if (!await HasHarvestAccess(dto.HarvestId))
            return Forbid();

        var farmId = GetCurrentFarmId();
        var workerId = GetCurrentWorkerId();
        
        var worker = await _workerRepository.GetByIdAsync(workerId, farmId);
        if (worker == null)
            return BadRequest(new { message = "Worker not found" });
        
        var adminId = worker.AdminId;
        
        var result = await _qualityCheckService.CreateQualityCheckAsync(dto, farmId, workerId, adminId);
        if (!result.Success)
            return BadRequest(result);
            
        return CreatedAtAction(nameof(GetById), new { id = result.Data?.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateQualityCheckDto dto)
    {
        var farmId = GetCurrentFarmId();
        var workerId = GetCurrentWorkerId();
        var result = await _qualityCheckService.UpdateOwnQualityCheckAsync(id, dto, workerId, farmId);
        if (!result.Success)
            return BadRequest(result);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var farmId = GetCurrentFarmId();
        var workerId = GetCurrentWorkerId();
        var result = await _qualityCheckService.DeleteOwnQualityCheckAsync(id, workerId, farmId);
        if (!result.Success)
            return BadRequest(result);
        return Ok(result);
    }

    [HttpPost("{id}/respond")]
    public async Task<IActionResult> RespondToAdmin(int id, [FromBody] QualityCheckWorkerResponseDto response)
    {
        var farmId = GetCurrentFarmId();
        var workerId = GetCurrentWorkerId();
        var result = await _qualityCheckService.RespondToAdminAsync(id, response, farmId, workerId);
        if (!result.Success)
            return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("pending-count")]
    public async Task<IActionResult> GetPendingCount()
    {
        var farmId = GetCurrentFarmId();
        var workerId = GetCurrentWorkerId();
        var hasPending = await _qualityCheckService.HasPendingApprovalsAsync(workerId, farmId);
        return Ok(new { HasPendingApprovals = hasPending });
    }

    [HttpGet("statistics/quality")]
    public async Task<IActionResult> GetQualityStatistics([FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
    {
        var farmId = GetCurrentFarmId();
        var workerId = GetCurrentWorkerId();
        var result = await _qualityCheckService.GetQualityStatisticsAsync(farmId, fromDate, toDate, workerId);
        return Ok(result);
    }
}