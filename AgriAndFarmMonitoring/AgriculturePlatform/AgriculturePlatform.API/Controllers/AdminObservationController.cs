// AgriculturePlatform.API/Controllers/AdminObservationController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using AgriculturePlatform.Application.DTOs.Observation;
using AgriculturePlatform.Application.Interfaces;
using AgriculturePlatform.API.Filters;
using AgriculturePlatform.API.Services;
using AgriculturePlatform.Application.Common;

namespace AgriculturePlatform.API.Controllers;

[ApiController]
[Route("api/admin/farms/{farmId}/observations")]
[Authorize]
[AuthorizeFarm]
public class AdminObservationController : ControllerBase
{
    private readonly IObservationService _observationService;
    private readonly ObservationStatisticsFormatter _statisticsFormatter;
    private readonly IFileStorageService _fileStorageService;

    public AdminObservationController(
        IObservationService observationService,
        ObservationStatisticsFormatter statisticsFormatter,
        IFileStorageService fileStorageService)
    {
        _observationService = observationService;
        _statisticsFormatter = statisticsFormatter;
        _fileStorageService = fileStorageService;
    }

    private int GetCurrentFarmId() => int.Parse(User.FindFirst("farmId")?.Value ?? "0");
    private int GetCurrentAdminId() => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

    // GET: api/admin/farms/{farmId}/observations
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] ObservationFilterDto filter)
    {
        var farmId = GetCurrentFarmId();
        var result = await _observationService.GetAllObservationsAsync(filter, farmId);
        return Ok(result);
    }

    // GET: api/admin/farms/{farmId}/observations/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var farmId = GetCurrentFarmId();
        var result = await _observationService.GetObservationByIdAsync(id, farmId);
        
        if (!result.Success)
            return NotFound(result);
            
        return Ok(result);
    }

    // GET: api/admin/farms/{farmId}/observations/field/{fieldId}
    [HttpGet("field/{fieldId}")]
    public async Task<IActionResult> GetByField(int fieldId)
    {
        var farmId = GetCurrentFarmId();
        var result = await _observationService.GetObservationsByFieldAsync(fieldId, farmId);
        return Ok(result);
    }

    // GET: api/admin/farms/{farmId}/observations/crop-cycle/{cropCycleId}
    [HttpGet("crop-cycle/{cropCycleId}")]
    public async Task<IActionResult> GetByCropCycle(int cropCycleId)
    {
        var farmId = GetCurrentFarmId();
        var result = await _observationService.GetObservationsByCropCycleAsync(cropCycleId, farmId);
        return Ok(result);
    }

    // GET: api/admin/farms/{farmId}/observations/worker/{workerId}
    [HttpGet("worker/{workerId}")]
    public async Task<IActionResult> GetByWorker(int workerId)
    {
        var farmId = GetCurrentFarmId();
        var result = await _observationService.GetObservationsByWorkerAsync(workerId, farmId);
        return Ok(result);
    }


// GET: api/admin/farms/{farmId}/observations/date-range
[HttpGet("date-range")]
public async Task<IActionResult> GetByDateRange([FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
{
    var farmId = GetCurrentFarmId();
    
    DateTime fromDateUtc;
    DateTime toDateUtc;
    
    if (fromDate.HasValue && toDate.HasValue)
    {
        // If only date is provided (no time), set to full day range
        if (fromDate.Value.TimeOfDay == TimeSpan.Zero && toDate.Value.TimeOfDay == TimeSpan.Zero)
        {
            fromDateUtc = fromDate.Value.ToUniversalTime().Date;
            toDateUtc = toDate.Value.ToUniversalTime().Date.AddDays(1).AddSeconds(-1);
        }
        else
        {
            fromDateUtc = fromDate.Value.ToUniversalTime();
            toDateUtc = toDate.Value.ToUniversalTime();
        }
    }
    else if (fromDate.HasValue)
    {
        fromDateUtc = fromDate.Value.ToUniversalTime().Date;
        toDateUtc = fromDateUtc.AddDays(1).AddSeconds(-1);
    }
    else if (toDate.HasValue)
    {
        toDateUtc = toDate.Value.ToUniversalTime().Date.AddDays(1).AddSeconds(-1);
        fromDateUtc = toDateUtc.AddDays(-7);
    }
    else
    {
        // Default to last 7 days
        toDateUtc = DateTime.UtcNow;
        fromDateUtc = toDateUtc.AddDays(-7);
    }
    
    var result = await _observationService.GetObservationsByDateRangeAsync(farmId, fromDateUtc, toDateUtc);
    return Ok(result);
}


    // GET: api/admin/farms/{farmId}/observations/statistics/pest


    
    [HttpGet("statistics/pest")]
    public async Task<IActionResult> GetPestStatistics(
        [FromQuery] DateTime? fromDate = null, 
        [FromQuery] DateTime? toDate = null,
        [FromQuery] string? format = null)
    {
        var farmId = GetCurrentFarmId();
        var result = await _observationService.GetPestStatisticsAsync(farmId, fromDate, toDate);
        
        if (!result.Success || result.Data == null)
            return NotFound(result);
        
        return format?.ToLower() switch
        {
            "simple" => Ok(new { result.Success, result.Message, Data = FormatSimple(result.Data) }),
            "chart" => Ok(new { result.Success, result.Message, Data = _statisticsFormatter.FormatForChartJs(result.Data) }),
            _ => Ok(new { result.Success, result.Message, Data = _statisticsFormatter.FormatForDisplay(result.Data) })
        };
    }
    
    private object FormatSimple(ObservationStatisticsDto data)
    {
        return new
        {
            data.TotalObservations,
            data.ObservationsWithPest,
            data.ObservationsWithoutPest,
            data.PestPercentage,
            TopPests = data.PestTypeDistribution.OrderByDescending(x => x.Value).Take(3)
        };
    }


    // PUT: api/admin/farms/{farmId}/observations/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateObservationDto dto)
    {
        var farmId = GetCurrentFarmId();
        var adminId = GetCurrentAdminId();
        var result = await _observationService.UpdateObservationAsync(id, dto, farmId, adminId);
        
        if (!result.Success)
            return NotFound(result);
            
        return Ok(result);
    }

    // DELETE: api/admin/farms/{farmId}/observations/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var farmId = GetCurrentFarmId();
        var adminId = GetCurrentAdminId();
        var result = await _observationService.DeleteObservationAsync(id, farmId, adminId);
        
        if (!result.Success)
            return BadRequest(result);
            
        return Ok(result);
    }


// GET: api/admin/farms/{farmId}/observations/pending-validation
[HttpGet("pending-validation")]
public async Task<IActionResult> GetPendingValidations([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
{
    var farmId = GetCurrentFarmId();
    var pagination = new PaginationParams { Page = page, PageSize = pageSize };
    var result = await _observationService.GetPendingValidationsAsync(farmId, pagination);
    return Ok(result);
}

// GET: api/admin/farms/{farmId}/observations/questioned
[HttpGet("questioned")]
public async Task<IActionResult> GetQuestionedObservations([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
{
    var farmId = GetCurrentFarmId();
    var pagination = new PaginationParams { Page = page, PageSize = pageSize };
    var result = await _observationService.GetQuestionedObservationsAsync(farmId, pagination);
    return Ok(result);
}

// POST: api/admin/farms/{farmId}/observations/{id}/validate
[HttpPost("{id}/validate")]
public async Task<IActionResult> ValidateObservation(int id, [FromBody] ObservationValidationDto validation)
{
    var farmId = GetCurrentFarmId();
    var adminId = GetCurrentAdminId();
    var result = await _observationService.ValidateObservationAsync(id, validation, farmId, adminId);
    
    if (!result.Success)
        return BadRequest(result);
        
    return Ok(result);
}

// GET: api/admin/farms/{farmId}/observations/statistics/validation-summary
[HttpGet("statistics/validation-summary")]
public async Task<IActionResult> GetValidationSummary()
{
    var farmId = GetCurrentFarmId();
    
    // Get all observations with pagination to get counts
    var allObservations = await _observationService.GetAllObservationsAsync(new ObservationFilterDto { PageSize = 1 }, farmId);
    
    // Get pending validations
    var pendingResult = await _observationService.GetPendingValidationsAsync(farmId, new PaginationParams { PageSize = 1 });
    
    // Get questioned observations
    var questionedResult = await _observationService.GetQuestionedObservationsAsync(farmId, new PaginationParams { PageSize = 1 });
    
    // Get verified observations (you need to add this method or calculate)
    var verifiedCount = await GetCountByValidationStatus(farmId, "verified");
    
    // Get invalid observations
    var invalidCount = await GetCountByValidationStatus(farmId, "invalid");
    
    return Ok(ApiResponse<object>.Ok(new
    {
        total = allObservations.Data?.TotalCount ?? 0,
        pending = pendingResult.Data?.TotalCount ?? 0,
        questioned = questionedResult.Data?.TotalCount ?? 0,
        verified = verifiedCount,
        invalid = invalidCount
    }));
}

// Helper method to get count by validation status
private async Task<int> GetCountByValidationStatus(int farmId, string validationStatus)
{
    var filter = new ObservationFilterDto 
    { 
        ValidationStatus = validationStatus,
        PageSize = 1 
    };
    var result = await _observationService.GetAllObservationsAsync(filter, farmId);
    return result.Data?.TotalCount ?? 0;
}

    // POST: api/admin/farms/{farmId}/observations/upload
    [HttpPost("upload")]
    public async Task<IActionResult> UploadImage(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { message = "No file uploaded." });
        }

        var extension = Path.GetExtension(file.FileName).ToLower();
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        if (!allowedExtensions.Contains(extension))
        {
            return BadRequest(new { message = "Invalid file type. Only JPG, PNG and WEBP are allowed." });
        }

        if (file.Length > 10 * 1024 * 1024)
        {
            return BadRequest(new { message = "File size exceeds limit (10MB)." });
        }

        var uniqueFileName = $"{Guid.NewGuid()}{extension}";

        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);
        var fileBytes = ms.ToArray();

        var relativePath = await _fileStorageService.SaveFileAsync(fileBytes, uniqueFileName, "observations");
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