// AgriculturePlatform.API/Controllers/WorkerObservationController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using AgriculturePlatform.Application.DTOs.Observation;
using AgriculturePlatform.Application.Interfaces;
using AgriculturePlatform.Application.Services;
using AgriculturePlatform.API.Filters;

namespace AgriculturePlatform.API.Controllers;

[ApiController]
[Route("api/worker/observations")]
[Authorize]
[AuthorizeWorkerFarm]
public class WorkerObservationController : ControllerBase
{
    private readonly IObservationService _observationService;
    private readonly IWorkerFieldAssignmentRepository _assignmentRepository;
    private readonly IWorkerRepository _workerRepository;
    private readonly IFileStorageService _fileStorageService;

    public WorkerObservationController(
        IObservationService observationService,
        IWorkerFieldAssignmentRepository assignmentRepository,
        IWorkerRepository workerRepository,
        IFileStorageService fileStorageService)
    {
        _observationService = observationService;
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

    // GET: api/worker/observations/my
    [HttpGet("my")]
    public async Task<IActionResult> GetMyObservations([FromQuery] ObservationFilterDto filter)
    {
        var farmId = GetCurrentFarmId();
        var workerId = GetCurrentWorkerId();
        filter.WorkerId = workerId;
        var result = await _observationService.GetAllObservationsAsync(filter, farmId);
        return Ok(result);
    }

    // GET: api/worker/observations/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var farmId = GetCurrentFarmId();
        var result = await _observationService.GetObservationByIdAsync(id, farmId);
        
        if (!result.Success)
            return NotFound(result);
            
        return Ok(result);
    }

    // POST: api/worker/observations
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateObservationDto dto)
    {
        // Verify worker has access to the field
        if (!await HasFieldAccess(dto.FieldId))
        {
            return Forbid();
        }

        var farmId = GetCurrentFarmId();
        var workerId = GetCurrentWorkerId();
        
        // Get the worker to retrieve the AdminId
        var worker = await _workerRepository.GetByIdAsync(workerId, farmId);
        if (worker == null)
        {
            return BadRequest(new { message = "Worker not found" });
        }
        
        var adminId = worker.AdminId;  // ← Get AdminId from the worker
        
        var result = await _observationService.CreateObservationAsync(dto, farmId, workerId, adminId);
        
        if (!result.Success)
            return BadRequest(result);
            
        return CreatedAtAction(nameof(GetById), new { id = result.Data?.Id }, result);
    }

    // PUT: api/worker/observations/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateObservationDto dto)
    {
        var farmId = GetCurrentFarmId();
        var workerId = GetCurrentWorkerId();
        var result = await _observationService.UpdateOwnObservationAsync(id, dto, workerId, farmId);
        
        if (!result.Success)
            return NotFound(result);
            
        return Ok(result);
    }

    // DELETE: api/worker/observations/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var farmId = GetCurrentFarmId();
        var workerId = GetCurrentWorkerId();
        var result = await _observationService.DeleteOwnObservationAsync(id, workerId, farmId);
        
        if (!result.Success)
            return BadRequest(result);
            
        return Ok(result);
    }

    // POST: api/worker/observations/{id}/respond
    [HttpPost("{id}/respond")]
    public async Task<IActionResult> RespondToAdmin(int id, [FromBody] ObservationWorkerResponseDto dto)
    {
        var farmId = GetCurrentFarmId();
        var workerId = GetCurrentWorkerId();
        
        var result = await _observationService.RespondToAdminAsync(id, dto, farmId, workerId);
        
        if (!result.Success)
            return BadRequest(result);
            
        return Ok(result);
    }

    // POST: api/worker/observations/upload
// API/Controllers/WorkerObservationController.cs - Update upload endpoint

// API/Controllers/WorkerObservationController.cs - Optimized Upload Endpoint

[HttpPost("upload")]
[RequestSizeLimit(10 * 1024 * 1024)] // 10MB
public async Task<IActionResult> UploadImage(IFormFile file)
{
    try
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "No file uploaded." });

        var extension = Path.GetExtension(file.FileName).ToLower();
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        if (!allowedExtensions.Contains(extension))
            return BadRequest(new { message = "Invalid file type. Only JPG, PNG and WEBP are allowed." });

        if (file.Length > 10 * 1024 * 1024)
            return BadRequest(new { message = "File size exceeds limit (10MB)." });

        var uniqueFileName = $"{Guid.NewGuid():N}{extension}";
        var subDirectory = $"observations/{DateTime.UtcNow:yyyy/MM/dd}";

        // ✅ Optimize: Read file directly from stream without copying to MemoryStream first
        using var stream = file.OpenReadStream();
        var fileBytes = new byte[file.Length];
        await stream.ReadAsync(fileBytes, 0, (int)file.Length);

        var relativePath = await _fileStorageService.SaveFileAsync(fileBytes, uniqueFileName, subDirectory);
        var publicUrl = _fileStorageService.GetPublicUrl(relativePath);

        return Ok(new
        {
            success = true,
            data = new
            {
                fileName = relativePath,
                url = publicUrl
            }
        });
    }
    catch (Exception ex)
    {
        return StatusCode(500, new { success = false, message = $"Upload failed: {ex.Message}" });
    }
}

// ✅ PATCH: api/worker/observations/{id}
[HttpPatch("{id}")]
public async Task<IActionResult> Patch(int id, [FromBody] UpdateObservationDto dto)
{
    var farmId = GetCurrentFarmId();
    var workerId = GetCurrentWorkerId();
    var result = await _observationService.PatchObservationAsync(id, dto, workerId, farmId);
    
    if (!result.Success)
        return BadRequest(result);
        
    return Ok(result);
}

}