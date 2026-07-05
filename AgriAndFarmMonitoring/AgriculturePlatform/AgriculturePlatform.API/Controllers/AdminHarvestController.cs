// API/Controllers/AdminHarvestController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using AgriculturePlatform.Application.Common;
using AgriculturePlatform.Application.DTOs.Harvest;
using AgriculturePlatform.Application.Interfaces;
using AgriculturePlatform.API.Filters;

namespace AgriculturePlatform.API.Controllers;

[ApiController]
[Route("api/admin/farms/{farmId}/harvests")]
[Authorize]
[AuthorizeFarm]
public class AdminHarvestController : ControllerBase
{
    private readonly IHarvestService _harvestService;
    private readonly IFileStorageService _fileStorageService;

    public AdminHarvestController(IHarvestService harvestService, IFileStorageService fileStorageService)
    {
        _harvestService = harvestService;
        _fileStorageService = fileStorageService;
    }

    private int GetCurrentFarmId() => int.Parse(User.FindFirst("farmId")?.Value ?? "0");
    private int GetCurrentAdminId() => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

    // GET: api/admin/farms/{farmId}/harvests
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] HarvestFilterDto filter)
    {
        var farmId = GetCurrentFarmId();
        var result = await _harvestService.GetAllHarvestsAsync(filter, farmId);
        return Ok(result);
    }

    // GET: api/admin/farms/{farmId}/harvests/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var farmId = GetCurrentFarmId();
        var result = await _harvestService.GetHarvestByIdAsync(id, farmId);
        
        if (!result.Success)
            return NotFound(result);
            
        return Ok(result);
    }

    // GET: api/admin/farms/{farmId}/harvests/field/{fieldId}
    [HttpGet("field/{fieldId}")]
    public async Task<IActionResult> GetByField(int fieldId)
    {
        var farmId = GetCurrentFarmId();
        var result = await _harvestService.GetHarvestsByFieldAsync(fieldId, farmId);
        return Ok(result);
    }

    // GET: api/admin/farms/{farmId}/harvests/crop-cycle/{cropCycleId}
    [HttpGet("crop-cycle/{cropCycleId}")]
    public async Task<IActionResult> GetByCropCycle(int cropCycleId)
    {
        var farmId = GetCurrentFarmId();
        var result = await _harvestService.GetHarvestsByCropCycleAsync(cropCycleId, farmId);
        return Ok(result);
    }

    // GET: api/admin/farms/{farmId}/harvests/worker/{workerId}
    [HttpGet("worker/{workerId}")]
    public async Task<IActionResult> GetByWorker(int workerId)
    {
        var farmId = GetCurrentFarmId();
        var result = await _harvestService.GetHarvestsByWorkerAsync(workerId, farmId);
        return Ok(result);
    }

    // GET: api/admin/farms/{farmId}/harvests/pending-approvals
    [HttpGet("pending-approvals")]
    public async Task<IActionResult> GetPendingApprovals([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var farmId = GetCurrentFarmId();
        var pagination = new PaginationParams { Page = page, PageSize = pageSize };
        var result = await _harvestService.GetPendingApprovalsAsync(farmId, pagination);
        return Ok(result);
    }

    // POST: api/admin/farms/{farmId}/harvests/{id}/approve
    [HttpPost("{id}/approve")]
    public async Task<IActionResult> ApproveHarvest(int id, [FromBody] HarvestApprovalDto approval)
    {
        var farmId = GetCurrentFarmId();
        var adminId = GetCurrentAdminId();
        var result = await _harvestService.ApproveHarvestAsync(id, approval, farmId, adminId);
        
        if (!result.Success)
            return BadRequest(result);
            
        return Ok(result);
    }

    // PUT: api/admin/farms/{farmId}/harvests/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateHarvestDto dto)
    {
        var farmId = GetCurrentFarmId();
        var adminId = GetCurrentAdminId();
        var result = await _harvestService.UpdateHarvestAsync(id, dto, farmId, adminId);
        
        if (!result.Success)
            return NotFound(result);
            
        return Ok(result);
    }

    // DELETE: api/admin/farms/{farmId}/harvests/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var farmId = GetCurrentFarmId();
        var adminId = GetCurrentAdminId();
        var result = await _harvestService.DeleteHarvestAsync(id, farmId, adminId);
        
        if (!result.Success)
            return BadRequest(result);
            
        return Ok(result);
    }

    // GET: api/admin/farms/{farmId}/harvests/statistics/yield
    [HttpGet("statistics/yield")]
    public async Task<IActionResult> GetYieldStatistics(
        [FromQuery] int? cropCycleId = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        var farmId = GetCurrentFarmId();
        var result = await _harvestService.GetYieldStatisticsAsync(farmId, cropCycleId, fromDate, toDate);
        return Ok(result);
    }

    // GET: api/admin/farms/{farmId}/harvests/statistics/comparison
    [HttpGet("statistics/comparison")]
    public async Task<IActionResult> GetYearOverYearComparison(
        [FromQuery] int currentYear,
        [FromQuery] int? previousYear = null)
    {
        var farmId = GetCurrentFarmId();
        var result = await _harvestService.GetYearOverYearComparisonAsync(farmId, currentYear, previousYear);
        return Ok(result);
    }

    // POST: api/admin/farms/{farmId}/harvests/upload
    [HttpPost("upload")]
    public async Task<IActionResult> UploadImage(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "No file uploaded." });

        var extension = Path.GetExtension(file.FileName).ToLower();
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        if (!allowedExtensions.Contains(extension))
            return BadRequest(new { message = "Invalid file type. Only JPG, PNG and WEBP are allowed." });

        if (file.Length > 10 * 1024 * 1024)
            return BadRequest(new { message = "File size exceeds limit (10MB)." });

        var uniqueFileName = $"{Guid.NewGuid()}{extension}";

        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);
        var fileBytes = ms.ToArray();

        var relativePath = await _fileStorageService.SaveFileAsync(fileBytes, uniqueFileName, "harvests");
        var fullUrl = _fileStorageService.GetDownloadUrl(relativePath);

        return Ok(new
        {
            success = true,
            data = new
            {
                fileName = relativePath,
                url = fullUrl
            }
        });
    }
}