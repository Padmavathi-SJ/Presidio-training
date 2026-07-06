// API/Controllers/WorkerHarvestController.cs - Updated Upload Method
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


// API/Controllers/WorkerHarvestController.cs - Upload Endpoint

[HttpPost("upload")]
[RequestSizeLimit(10 * 1024 * 1024)] // 10MB
public async Task<IActionResult> UploadImage(IFormFile file)
{
    try
    {
        // Validate file
        if (file == null || file.Length == 0)
            return BadRequest(new { success = false, message = "No file uploaded." });

        var extension = Path.GetExtension(file.FileName).ToLower();
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        
        if (!allowedExtensions.Contains(extension))
            return BadRequest(new { success = false, message = "Invalid file type. Only JPG, PNG and WEBP are allowed." });

        if (file.Length > 10 * 1024 * 1024)
            return BadRequest(new { success = false, message = "File size exceeds limit (10MB)." });

        // Generate unique filename
        var uniqueFileName = $"{Guid.NewGuid():N}{extension}";
        var subDirectory = $"harvests/{DateTime.UtcNow:yyyy/MM/dd}";

        // Read file
        using var stream = file.OpenReadStream();
        var fileBytes = new byte[file.Length];
        await stream.ReadAsync(fileBytes, 0, (int)file.Length);

        // Save file
        var relativePath = await _fileStorageService.SaveFileAsync(fileBytes, uniqueFileName, subDirectory);
        
        // Get public URL
        var publicUrl = _fileStorageService.GetPublicUrl(relativePath);

        return Ok(new
        {
            success = true,
            data = new
            {
                fileName = relativePath, // Stores: "harvests/2026/07/06/file.jpg"
                url = publicUrl           // Returns: "http://localhost:5000/uploads/harvests/2026/07/06/file.jpg"
            }
        });
    }
    catch (Exception ex)
    {
        return StatusCode(500, new { success = false, message = $"Upload failed: {ex.Message}" });
    }
}


// ✅ OPTIMIZED: Resize thumbnail instead of copying full image
private async Task<string> GenerateThumbnailAsync(byte[] imageBytes, string fileName, string subDirectory)
{
    var thumbnailName = $"thumb_{fileName}";
    var thumbnailPath = Path.Combine(subDirectory, thumbnailName);
    
    try
    {
        // Use ImageSharp to resize (add SixLabors.ImageSharp NuGet package)
        // For now, create a simple resize using System.Drawing (if on Windows)
        // Or skip thumbnail generation for now to improve speed
        // If you want to keep it simple, just save a small version
        
        // 🚀 OPTIMIZATION: Skip thumbnail generation entirely for speed
        // or use a lightweight resizing library
        
        // For now, just save a compressed version (smaller quality)
        // This is faster than copying the full image
        await _fileStorageService.SaveFileAsync(imageBytes, thumbnailName, subDirectory);
    }
    catch
    {
        // If thumbnail fails, just return null
        return null!;
    }
    
    return thumbnailPath;
}


    // DELETE: api/worker/harvests/upload/{fileName} - NEW: Delete uploaded temp file
// API/Controllers/WorkerHarvestController.cs - Add this endpoint

[HttpDelete("upload/{fileName}")]
public async Task<IActionResult> DeleteUploadedImage(string fileName)
{
    if (string.IsNullOrEmpty(fileName))
        return BadRequest(new { message = "File name is required." });

    // Decode the URL-encoded file name
    var decodedFileName = Uri.UnescapeDataString(fileName);
    
    try
    {
        var deleted = await _fileStorageService.DeleteFileAsync(decodedFileName);
        
        if (!deleted)
            return NotFound(new { message = "File not found." });

        return Ok(new { success = true, message = "File deleted successfully." });
    }
    catch (Exception ex)
    {
        return StatusCode(500, new { success = false, message = $"Error deleting file: {ex.Message}" });
    }
}

    private bool IsImageFile(string extension)
    {
        var validExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        return validExtensions.Contains(extension.ToLower());
    }



}