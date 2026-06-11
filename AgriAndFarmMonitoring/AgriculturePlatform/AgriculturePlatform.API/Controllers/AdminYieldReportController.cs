// API/Controllers/AdminYieldReportController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using AgriculturePlatform.Application.Common;
using AgriculturePlatform.Application.DTOs.YieldReport;
using AgriculturePlatform.Application.Interfaces;
using AgriculturePlatform.API.Filters;

namespace AgriculturePlatform.API.Controllers;

[ApiController]
[Route("api/admin/farms/{farmId}/yield-reports")]
[Authorize(Roles = "Admin")]
[AuthorizeFarm]
public class AdminYieldReportController : ControllerBase
{
    private readonly IYieldReportService _reportService;

    public AdminYieldReportController(IYieldReportService reportService)
    {
        _reportService = reportService;
    }

    private int GetCurrentFarmId() => int.Parse(User.FindFirst("farmId")?.Value ?? "0");
    private int GetCurrentAdminId() => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] YieldReportFilterDto filter)
    {
        var farmId = GetCurrentFarmId();
        var result = await _reportService.GetAllReportsAsync(filter, farmId);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var farmId = GetCurrentFarmId();
        var result = await _reportService.GetReportByIdAsync(id, farmId);
        if (!result.Success)
            return NotFound(result);
        return Ok(result);
    }

    [HttpPost("generate")]
    public async Task<IActionResult> GenerateReport([FromBody] GenerateYieldReportDto dto)
    {
        var farmId = GetCurrentFarmId();
        var adminId = GetCurrentAdminId();
        var result = await _reportService.GenerateReportAsync(dto, farmId, adminId);
        if (!result.Success)
            return BadRequest(result);
        return Ok(result);
    }

    [HttpPost("schedule")]
    public async Task<IActionResult> CreateScheduledReport([FromBody] CreateYieldReportDto dto)
    {
        var farmId = GetCurrentFarmId();
        var adminId = GetCurrentAdminId();
        var result = await _reportService.CreateScheduledReportAsync(dto, farmId, adminId);
        if (!result.Success)
            return BadRequest(result);
        return Ok(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateYieldReportDto dto)
    {
        var farmId = GetCurrentFarmId();
        var adminId = GetCurrentAdminId();
        var result = await _reportService.UpdateReportAsync(id, dto, farmId, adminId);
        if (!result.Success)
            return NotFound(result);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var farmId = GetCurrentFarmId();
        var adminId = GetCurrentAdminId();
        var result = await _reportService.DeleteReportAsync(id, farmId, adminId);
        if (!result.Success)
            return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("{id}/export")]
    public async Task<IActionResult> ExportReport(int id, [FromQuery] string format = "CSV")
    {
        var farmId = GetCurrentFarmId();
        var adminId = GetCurrentAdminId();
        var result = await _reportService.ExportReportAsync(id, format, farmId, adminId);
        
        if (!result.Success)
            return BadRequest(result);

        return Ok(new
    {
        result.Success,
        result.Message,
        DownloadInfo = result.Data
    });

       
    }

    [HttpGet("statistics/comparison")]
    public async Task<IActionResult> CompareYields(
        [FromQuery] int? fieldId,
        [FromQuery] int currentYear,
        [FromQuery] int? previousYear)
    {
        var farmId = GetCurrentFarmId();
        var result = await _reportService.CompareYieldsAsync(farmId, fieldId, currentYear, previousYear);
        return Ok(result);
    }

    [HttpGet("statistics/summary")]
    public async Task<IActionResult> GetYieldSummary(
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate)
    {
        var farmId = GetCurrentFarmId();
        var result = await _reportService.GetYieldSummaryAsync(farmId, fromDate, toDate);
        return Ok(result);
    }

    [HttpGet("by-crop-type/{cropType}")]
    public async Task<IActionResult> GetByCropType(string cropType, [FromQuery] int? year)
    {
        var farmId = GetCurrentFarmId();
        var startDate = year.HasValue ? new DateTime(year.Value, 1, 1) : DateTime.UtcNow.AddYears(-1);
        var endDate = year.HasValue ? new DateTime(year.Value, 12, 31) : DateTime.UtcNow;
        
        var filter = new YieldReportFilterDto
        {
            FromDate = startDate,
            ToDate = endDate
        };
        
        var result = await _reportService.GetAllReportsAsync(filter, farmId);
        
        // Filter reports by crop type
        var filtered = result.Data?.Items.Where(r => r.CropType == cropType).ToList();
        
        return Ok(filtered);
    }

    [HttpGet("by-season/{season}/{year}")]
    public async Task<IActionResult> GetBySeason(string season, int year)
    {
        var farmId = GetCurrentFarmId();
        
        var (startDate, endDate) = GetSeasonDateRange(season, year);
        var filter = new YieldReportFilterDto
        {
            FromDate = startDate,
            ToDate = endDate
        };
        
        var result = await _reportService.GetAllReportsAsync(filter, farmId);
        return Ok(result);
    }

    private (DateTime, DateTime) GetSeasonDateRange(string season, int year)
    {
        return season.ToUpper() switch
        {
            "SPRING" => (new DateTime(year, 3, 1), new DateTime(year, 5, 31)),
            "SUMMER" => (new DateTime(year, 6, 1), new DateTime(year, 8, 31)),
            "FALL" => (new DateTime(year, 9, 1), new DateTime(year, 11, 30)),
            "WINTER" => (new DateTime(year, 12, 1), new DateTime(year + 1, 2, 28)),
            _ => (new DateTime(year, 1, 1), new DateTime(year, 12, 31))
        };
    }
}