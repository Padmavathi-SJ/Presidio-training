// AgriculturePlatform.API/Controllers/AdminWorkerFieldController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using AgriculturePlatform.Application.DTOs.WorkerField;
using AgriculturePlatform.Application.Interfaces;
using AgriculturePlatform.API.Filters;

namespace AgriculturePlatform.API.Controllers;

[ApiController]
[Route("api/admin/farms/{farmId}/worker-fields")]
[Authorize]
[AuthorizeFarm]
public class AdminWorkerFieldController : ControllerBase
{
    private readonly IWorkerFieldAssignmentService _assignmentService;

    public AdminWorkerFieldController(IWorkerFieldAssignmentService assignmentService)
    {
        _assignmentService = assignmentService;
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
                        ?? User.FindFirst("adminId")?.Value;
        
        if (string.IsNullOrEmpty(adminIdClaim))
        {
            Console.WriteLine("Admin ID claim not found in token");
            return 0;
        }
        
        Console.WriteLine($"Admin ID claim found: {adminIdClaim}");
        return int.TryParse(adminIdClaim, out var id) ? id : 0;
    }

    private string GetIpAddress()
    {
        return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
    }

    private string GetUserAgent()
    {
        return HttpContext.Request.Headers["User-Agent"].ToString() ?? "Unknown";
    }

    // GET: api/admin/farms/{farmId}/worker-fields
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] WorkerFieldFilterDto filter)
    {
        var farmId = GetCurrentFarmId();
        var result = await _assignmentService.GetAllAssignmentsAsync(filter, farmId);
        return Ok(result);
    }

    // GET: api/admin/farms/{farmId}/worker-fields/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var farmId = GetCurrentFarmId();
        // Note: You may need to add a GetAssignmentById method to your service if needed
        // For now, this endpoint is not implemented
        return Ok(new { message = "Get by id endpoint - implement if needed" });
    }

    // POST: api/admin/farms/{farmId}/worker-fields
    [HttpPost]
    public async Task<IActionResult> AssignField([FromBody] AssignFieldToWorkerDto dto)
    {
        var farmId = GetCurrentFarmId();
        var adminId = GetCurrentAdminId();
        var ipAddress = GetIpAddress();
        var userAgent = GetUserAgent();
        
        // Debug: Log the adminId
        Console.WriteLine($"FarmId: {farmId}, AdminId: {adminId}, FieldId: {dto.FieldId}, WorkerId: {dto.WorkerId}");
        
        var result = await _assignmentService.AssignFieldToWorkerAsync(dto, farmId, adminId, ipAddress, userAgent);
        
        if (!result.Success)
            return BadRequest(result);
            
        return Ok(result);
    }

    // PUT: api/admin/farms/{farmId}/worker-fields/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAssignment(int id, [FromBody] AssignFieldToWorkerDto dto)
    {
        var farmId = GetCurrentFarmId();
        var adminId = GetCurrentAdminId();
        var ipAddress = GetIpAddress();
        var userAgent = GetUserAgent();
        
        var result = await _assignmentService.UpdateAssignmentAsync(id, dto, farmId, adminId, ipAddress, userAgent);
        
        if (!result.Success)
            return NotFound(result);
            
        return Ok(result);
    }

    // DELETE: api/admin/farms/{farmId}/worker-fields/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> RemoveAssignment(int id)
    {
        var farmId = GetCurrentFarmId();
        var adminId = GetCurrentAdminId();
        var ipAddress = GetIpAddress();
        var userAgent = GetUserAgent();
        
        var result = await _assignmentService.RemoveAssignmentAsync(id, farmId, adminId, ipAddress, userAgent);
        
        if (!result.Success)
            return BadRequest(result);
            
        return Ok(result);
    }

    // GET: api/admin/farms/{farmId}/worker-fields/worker/{workerId}
    [HttpGet("worker/{workerId}")]
    public async Task<IActionResult> GetByWorker(int workerId, [FromQuery] WorkerFieldFilterDto filter)
    {
        var farmId = GetCurrentFarmId();
        filter.WorkerId = workerId;
        var result = await _assignmentService.GetAllAssignmentsAsync(filter, farmId);
        return Ok(result);
    }

    // GET: api/admin/farms/{farmId}/worker-fields/field/{fieldId}
    [HttpGet("field/{fieldId}")]
    public async Task<IActionResult> GetByField(int fieldId, [FromQuery] WorkerFieldFilterDto filter)
    {
        var farmId = GetCurrentFarmId();
        filter.FieldId = fieldId;
        var result = await _assignmentService.GetAllAssignmentsAsync(filter, farmId);
        return Ok(result);
    }

    // GET: api/admin/farms/{farmId}/worker-fields/active
    [HttpGet("active")]
    public async Task<IActionResult> GetActiveAssignments([FromQuery] WorkerFieldFilterDto filter)
    {
        var farmId = GetCurrentFarmId();
        filter.IsActive = true;
        var result = await _assignmentService.GetAllAssignmentsAsync(filter, farmId);
        return Ok(result);
    }
}