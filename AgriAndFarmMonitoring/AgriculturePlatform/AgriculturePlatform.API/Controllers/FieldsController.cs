// AgriculturePlatform.API/Controllers/FieldsController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims; 
using AgriculturePlatform.Application.Common;
using AgriculturePlatform.Application.DTOs.Field;
using AgriculturePlatform.Application.Interfaces;
using AgriculturePlatform.API.Filters;

namespace AgriculturePlatform.API.Controllers;

[ApiController]
[Route("api/farms/{farmId}/[controller]")]
[Authorize]
[AuthorizeFarm]
public class FieldsController : ControllerBase
{
    private readonly IFieldService _fieldService;
    private readonly IExcelService _excelService;

    public FieldsController(IFieldService fieldService, IExcelService excelService)
    {
        _fieldService = fieldService;
        _excelService = excelService;
    }

    private int GetCurrentFarmId()
    {
        return int.Parse(User.FindFirst("farmId")?.Value ?? "0");
    }

    private int GetCurrentAdminId()
    {
        // Try different claim types
        var adminIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                        ?? User.FindFirst("sub")?.Value 
                        ?? User.FindFirst("id")?.Value;
        
        if (string.IsNullOrEmpty(adminIdClaim))
        {
            Console.WriteLine("Admin ID claim not found in token");
            return 0;
        }
        
        return int.Parse(adminIdClaim);
    }
    
    private string GetIpAddress()
    {
        return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
    }
    
    private string GetUserAgent()
    {
        return HttpContext.Request.Headers["User-Agent"].ToString() ?? "Unknown";
    }

    // GET: api/farms/{farmId}/fields
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] FieldFilterDto filter)
    {
        var farmId = GetCurrentFarmId();
        Console.WriteLine("=== DEBUG: All Claims ===");
    foreach (var claim in User.Claims)
    {
        Console.WriteLine($"{claim.Type}: {claim.Value}");
    }
    Console.WriteLine($"=== FarmId from token: {farmId} ===");

        var result = await _fieldService.GetAllAsync(filter, farmId);
        if (result.Success && result.Data != null)
    {
        Console.WriteLine($"✅ Returning {result.Data.TotalCount} fields for Farm {farmId}");
    }
    
        return Ok(result);
    }

    // GET: api/farms/{farmId}/fields/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var farmId = GetCurrentFarmId();
        var result = await _fieldService.GetByIdAsync(id, farmId);
        
        if (!result.Success)
            return NotFound(result);
            
        return Ok(result);
    }

    // GET: api/farms/{farmId}/fields/statistics
    [HttpGet("statistics")]
    public async Task<IActionResult> GetStatistics()
    {
        var farmId = GetCurrentFarmId();
        var result = await _fieldService.GetStatisticsAsync(farmId);
        return Ok(result);
    }

    // POST: api/farms/{farmId}/fields
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateFieldDto dto)
    {
        var farmId = GetCurrentFarmId();
        var adminId = GetCurrentAdminId();
        Console.WriteLine($"Admin ID from token: {adminId}");
        var ipAddress = GetIpAddress();
        var userAgent = GetUserAgent();
        
        var result = await _fieldService.CreateAsync(dto, farmId, adminId, ipAddress, userAgent);
        
        if (!result.Success)
            return BadRequest(result);
            
        return CreatedAtAction(nameof(GetById), new { farmId, id = result.Data?.Id }, result);
    }

    // PUT: api/farms/{farmId}/fields/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateFieldDto dto)
    {
        var farmId = GetCurrentFarmId();
        var adminId = GetCurrentAdminId();
        var ipAddress = GetIpAddress();
        var userAgent = GetUserAgent();
        
        var result = await _fieldService.UpdateAsync(id, dto, farmId, adminId, ipAddress, userAgent);
        
        if (!result.Success)
            return NotFound(result);
            
        return Ok(result);
    }

    // PUT: api/farms/{farmId}/fields/{id}/location - NEW ENDPOINT
    [HttpPut("{id}/location")]
    public async Task<IActionResult> UpdateLocation(int id, [FromBody] UpdateLocationDto dto)
    {
        var farmId = GetCurrentFarmId();
        var adminId = GetCurrentAdminId();
        var ipAddress = GetIpAddress();
        var userAgent = GetUserAgent();
        
        var result = await _fieldService.UpdateFieldLocationAsync(id, dto.Latitude, dto.Longitude, farmId, adminId, ipAddress, userAgent);
        
        if (!result.Success)
            return BadRequest(result);
            
        return Ok(result);
    }

    // DELETE: api/farms/{farmId}/fields/{id} (Soft Delete)
    [HttpDelete("{id}")]
    public async Task<IActionResult> SoftDelete(int id)
    {
        var farmId = GetCurrentFarmId();
        var adminId = GetCurrentAdminId();
        var ipAddress = GetIpAddress();
        var userAgent = GetUserAgent();
        
        var result = await _fieldService.SoftDeleteAsync(id, farmId, adminId, ipAddress, userAgent);
        
        if (!result.Success)
            return BadRequest(result);
            
        return Ok(result);
    }
    
    // POST: api/farms/{farmId}/fields/bulk-import
    [HttpPost("bulk-import")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<IActionResult> BulkImport(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "Please upload a valid Excel file" });

        var allowedExtensions = new[] { ".xlsx", ".xls" };
        var extension = Path.GetExtension(file.FileName).ToLower();
        
        if (!allowedExtensions.Contains(extension))
            return BadRequest(new { message = "Only Excel files (.xlsx, .xls) are allowed" });

        var farmId = GetCurrentFarmId();
        var adminId = GetCurrentAdminId();
        var ipAddress = GetIpAddress();
        var userAgent = GetUserAgent();
        
        using var stream = file.OpenReadStream();
        var result = await _fieldService.BulkImportAsync(stream, farmId, adminId, ipAddress, userAgent);
        
        return Ok(result);
    }
    
    // POST: api/farms/{farmId}/fields/bulk-soft-delete
    [HttpPost("bulk-soft-delete")]
    public async Task<IActionResult> BulkSoftDelete([FromBody] List<int> ids)
    {
        var farmId = GetCurrentFarmId();
        var adminId = GetCurrentAdminId();
        var ipAddress = GetIpAddress();
        var userAgent = GetUserAgent();
        
        var result = await _fieldService.BulkSoftDeleteAsync(ids, farmId, adminId, ipAddress, userAgent);
        return Ok(result);
    }
    
    // GET: api/farms/{farmId}/fields/export
    [HttpGet("export")]
    public async Task<IActionResult> ExportToExcel()
    {
        var farmId = GetCurrentFarmId();
        var result = await _fieldService.ExportToExcelAsync(farmId);
        
        if (!result.Success)
            return BadRequest(result);
            
        return File(result.Data!, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "fields_export.xlsx");
    }

    // GET: api/farms/{farmId}/fields/template
    [HttpGet("template")]
    public IActionResult GetTemplate()
    {
        var template = _excelService.CreateExcelTemplate();
        return File(template, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "fields_import_template.xlsx");
    }
}

// UpdateLocationDto - Add this class at the end of the file or in a separate file
public class UpdateLocationDto
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}