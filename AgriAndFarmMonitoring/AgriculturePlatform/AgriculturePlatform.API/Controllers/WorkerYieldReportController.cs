// API/Controllers/WorkerYieldReportController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using AgriculturePlatform.Application.DTOs.YieldReport;
using AgriculturePlatform.Application.Interfaces;
using AgriculturePlatform.API.Filters;

namespace AgriculturePlatform.API.Controllers;

[ApiController]
[Route("api/worker/yield-reports")]
[Authorize]
[AuthorizeWorkerFarm]
public class WorkerYieldReportController : ControllerBase
{
    private readonly IYieldReportService _reportService;

    public WorkerYieldReportController(IYieldReportService reportService)
    {
        _reportService = reportService;
    }

    private int GetCurrentFarmId() => int.Parse(User.FindFirst("farmId")?.Value ?? "0");
    private int GetCurrentWorkerId() => int.Parse(User.FindFirst("workerId")?.Value ?? "0");

    [HttpGet]
    public async Task<IActionResult> GetMyReports([FromQuery] YieldReportFilterDto filter)
    {
        var farmId = GetCurrentFarmId();
        var workerId = GetCurrentWorkerId();
        var result = await _reportService.GetReportsForWorkerAsync(filter, farmId, workerId);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var farmId = GetCurrentFarmId();
        var workerId = GetCurrentWorkerId();
        var result = await _reportService.GetReportByIdForWorkerAsync(id, farmId, workerId);
        
        if (!result.Success)
            return NotFound(result);
            
        return Ok(result);
    }

    [HttpPost("generate")]
    public async Task<IActionResult> GenerateReport([FromBody] GenerateYieldReportDto dto)
    {
        var farmId = GetCurrentFarmId();
        var workerId = GetCurrentWorkerId();
        // Since workers can only generate reports for their own fields, the service should enforce this, 
        // but for now we reuse the admin method and pass workerId as adminId for tracking who created it.
        // Actually, the IYieldReportService expects an adminId. We can pass workerId.
        var result = await _reportService.GenerateReportAsync(dto, farmId, workerId);
        if (!result.Success)
            return BadRequest(result);
        return Ok(result);
    }
}