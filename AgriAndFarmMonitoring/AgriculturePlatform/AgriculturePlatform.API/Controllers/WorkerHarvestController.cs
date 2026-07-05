// API/Controllers/WorkerHarvestController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using AgriculturePlatform.Application.Common;
using AgriculturePlatform.Application.DTOs.Harvest;
using AgriculturePlatform.Application.Interfaces;
using AgriculturePlatform.API.Filters;

namespace AgriculturePlatform.API.Controllers;

[ApiController]
[Route("api/worker/harvests")]
[Authorize]
[AuthorizeWorkerFarm]
public class WorkerHarvestController : ControllerBase
{
    private readonly IHarvestService _harvestService;
    private readonly IWorkerFieldAssignmentRepository _assignmentRepository;
    private readonly IWorkerRepository _workerRepository;
    private readonly IFileStorageService _fileStorageService;

    public WorkerHarvestController(
        IHarvestService harvestService,
        IWorkerFieldAssignmentRepository assignmentRepository,
        IWorkerRepository workerRepository,
        IFileStorageService fileStorageService)
    {
        _harvestService = harvestService;
        _assignmentRepository = assignmentRepository;
        _workerRepository = workerRepository;
        _fileStorageService = fileStorageService;
    }

    private int GetCurrentFarmId() => int.Parse(User.FindFirst("farmId")?.Value ?? "0");
    private int GetCurrentWorkerId() => int.Parse(User.FindFirst("workerId")?.Value ?? "0");

    private async Task<bool> HasFieldAccess(int fieldId)
    {
        return await _assignmentRepository.HasWorkerAccessToFieldAsync(GetCurrentWorkerId(), fieldId, GetCurrentFarmId());
    }

    // GET: api/worker/harvests/my
    [HttpGet("my")]
    public async Task<IActionResult> GetMyHarvests([FromQuery] HarvestFilterDto filter)
    {
        var farmId = GetCurrentFarmId();
        var workerId = GetCurrentWorkerId();
        filter.WorkerId = workerId;
        var result = await _harvestService.GetAllHarvestsAsync(filter, farmId);
        return Ok(result);
    }

    // GET: api/worker/harvests/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var farmId = GetCurrentFarmId();
        var result = await _harvestService.GetHarvestByIdAsync(id, farmId);
        
        if (!result.Success)
            return NotFound(result);
            
        return Ok(result);
    }

    // POST: api/worker/harvests
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateHarvestDto dto)
    {
        if (!await HasFieldAccess(dto.FieldId))
            return Forbid();

        var farmId = GetCurrentFarmId();
        var workerId = GetCurrentWorkerId();
        
        var worker = await _workerRepository.GetByIdAsync(workerId, farmId);
        if (worker == null)
            return BadRequest(new { message = "Worker not found" });
        
        var adminId = worker.AdminId;
        
        var result = await _harvestService.CreateHarvestAsync(dto, farmId, workerId, adminId);
        
        if (!result.Success)
            return BadRequest(result);
            
        return CreatedAtAction(nameof(GetById), new { id = result.Data?.Id }, result);
    }

    // PUT: api/worker/harvests/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateHarvestDto dto)
    {
        var farmId = GetCurrentFarmId();
        var workerId = GetCurrentWorkerId();
        var result = await _harvestService.UpdateOwnHarvestAsync(id, dto, workerId, farmId);
        
        if (!result.Success)
            return BadRequest(result);
            
        return Ok(result);
    }

    // DELETE: api/worker/harvests/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var farmId = GetCurrentFarmId();
        var workerId = GetCurrentWorkerId();
        var result = await _harvestService.DeleteOwnHarvestAsync(id, workerId, farmId);
        
        if (!result.Success)
            return BadRequest(result);
            
        return Ok(result);
    }

    // POST: api/worker/harvests/{id}/respond
    [HttpPost("{id}/respond")]
    public async Task<IActionResult> RespondToAdmin(int id, [FromBody] HarvestWorkerResponseDto response)
    {
        var farmId = GetCurrentFarmId();
        var workerId = GetCurrentWorkerId();
        var result = await _harvestService.RespondToAdminAsync(id, response, farmId, workerId);
        
        if (!result.Success)
            return BadRequest(result);
            
        return Ok(result);
    }

    // GET: api/worker/harvests/pending-count
    [HttpGet("pending-count")]
    public async Task<IActionResult> GetPendingCount()
    {
        var farmId = GetCurrentFarmId();
        var workerId = GetCurrentWorkerId();
        var hasPending = await _harvestService.HasPendingApprovalsAsync(workerId, farmId);
        
        return Ok(new { HasPendingApprovals = hasPending });
    }

    // POST: api/worker/harvests/upload
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