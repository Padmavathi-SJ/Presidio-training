// API/Controllers/AdminQualityCheckController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using AgriculturePlatform.Application.Common;
using AgriculturePlatform.Application.DTOs.QualityCheck;
using AgriculturePlatform.Application.Interfaces;
using AgriculturePlatform.API.Filters;

namespace AgriculturePlatform.API.Controllers;

[ApiController]
[Route("api/admin/farms/{farmId}/quality-checks")]
[Authorize(Roles = "Admin")]
[AuthorizeFarm]
public class AdminQualityCheckController : ControllerBase
{
    private readonly IQualityCheckService _qualityCheckService;

    public AdminQualityCheckController(IQualityCheckService qualityCheckService)
    {
        _qualityCheckService = qualityCheckService;
    }

    private int GetCurrentFarmId() => int.Parse(User.FindFirst("farmId")?.Value ?? "0");
    private int GetCurrentAdminId() => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] QualityCheckFilterDto filter)
    {
        var farmId = GetCurrentFarmId();
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

    [HttpGet("harvest/{harvestId}")]
    public async Task<IActionResult> GetByHarvest(int harvestId)
    {
        var farmId = GetCurrentFarmId();
        var result = await _qualityCheckService.GetQualityChecksByHarvestAsync(harvestId, farmId);
        return Ok(result);
    }

    [HttpGet("worker/{workerId}")]
    public async Task<IActionResult> GetByWorker(int workerId)
    {
        var farmId = GetCurrentFarmId();
        var result = await _qualityCheckService.GetQualityChecksByWorkerAsync(workerId, farmId);
        return Ok(result);
    }

    [HttpGet("pending-approvals")]
    public async Task<IActionResult> GetPendingApprovals([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var farmId = GetCurrentFarmId();
        var pagination = new PaginationParams { Page = page, PageSize = pageSize };
        var result = await _qualityCheckService.GetPendingApprovalsAsync(farmId, pagination);
        return Ok(result);
    }

    [HttpPost("{id}/approve")]
    public async Task<IActionResult> ApproveQualityCheck(int id, [FromBody] QualityCheckApprovalDto approval)
    {
        var farmId = GetCurrentFarmId();
        var adminId = GetCurrentAdminId();
        var result = await _qualityCheckService.ApproveQualityCheckAsync(id, approval, farmId, adminId);
        if (!result.Success)
            return BadRequest(result);
        return Ok(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateQualityCheckDto dto)
    {
        var farmId = GetCurrentFarmId();
        var adminId = GetCurrentAdminId();
        var result = await _qualityCheckService.UpdateQualityCheckAsync(id, dto, farmId, adminId);
        if (!result.Success)
            return NotFound(result);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var farmId = GetCurrentFarmId();
        var adminId = GetCurrentAdminId();
        var result = await _qualityCheckService.DeleteQualityCheckAsync(id, farmId, adminId);
        if (!result.Success)
            return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("statistics/quality")]
    public async Task<IActionResult> GetQualityStatistics([FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
    {
        var farmId = GetCurrentFarmId();
        var result = await _qualityCheckService.GetQualityStatisticsAsync(farmId, fromDate, toDate);
        return Ok(result);
    }
}