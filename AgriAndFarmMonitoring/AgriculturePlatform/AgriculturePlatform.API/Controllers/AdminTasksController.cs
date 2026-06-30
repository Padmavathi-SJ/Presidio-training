// AgriculturePlatform.API/Controllers/AdminTasksController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using AgriculturePlatform.Application.DTOs.Task;
using AgriculturePlatform.Application.Interfaces;
using AgriculturePlatform.API.Filters;

namespace AgriculturePlatform.API.Controllers;

[ApiController]
[Route("api/admin/farms/{farmId}/tasks")]
[Authorize]
[AuthorizeFarm]
public class AdminTasksController : ControllerBase
{
    private readonly ITaskService _taskService;
    private readonly IExcelTaskService _excelTaskService;

    public AdminTasksController(ITaskService taskService, IExcelTaskService excelTaskService)
    {
        _taskService = taskService;
        _excelTaskService = excelTaskService;
    }

    private int GetCurrentFarmId() => int.Parse(User.FindFirst("farmId")?.Value ?? "0");
    private int GetCurrentAdminId() => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

    // =============================================
    // QUERY ENDPOINTS
    // =============================================

    // GET: api/admin/farms/{farmId}/tasks
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] TaskFilterDto filter)
    {
        var farmId = GetCurrentFarmId();
        var result = await _taskService.GetAllAsync(filter, farmId);
        return Ok(result);
    }

    // GET: api/admin/farms/{farmId}/tasks/active
    [HttpGet("active")]
    public async Task<IActionResult> GetActiveTasks()
    {
        var farmId = GetCurrentFarmId();
        var result = await _taskService.GetActiveTasksAsync(farmId);
        return Ok(result);
    }

    // GET: api/admin/farms/{farmId}/tasks/overdue
    [HttpGet("overdue")]
    public async Task<IActionResult> GetOverdueTasks()
    {
        var farmId = GetCurrentFarmId();
        var result = await _taskService.GetOverdueTasksAsync(farmId);
        return Ok(result);
    }

    // GET: api/admin/farms/{farmId}/tasks/worker/{workerId}
    [HttpGet("worker/{workerId}")]
    public async Task<IActionResult> GetByWorker(int workerId)
    {
        var farmId = GetCurrentFarmId();
        var result = await _taskService.GetTasksByWorkerAsync(workerId, farmId);
        return Ok(result);
    }

    // GET: api/admin/farms/{farmId}/tasks/field/{fieldId}
    [HttpGet("field/{fieldId}")]
    public async Task<IActionResult> GetByField(int fieldId)
    {
        var farmId = GetCurrentFarmId();
        var result = await _taskService.GetTasksByFieldAsync(fieldId, farmId);
        return Ok(result);
    }

    // GET: api/admin/farms/{farmId}/tasks/statistics
    [HttpGet("statistics")]
    public async Task<IActionResult> GetStatistics()
    {
        var farmId = GetCurrentFarmId();
        var result = await _taskService.GetTaskStatisticsAsync(farmId);
        return Ok(result);
    }

    // GET: api/admin/farms/{farmId}/tasks/completion-history
    [HttpGet("completion-history")]
    public async Task<IActionResult> GetCompletionHistory([FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
    {
        var farmId = GetCurrentFarmId();
        var result = await _taskService.GetTaskCompletionHistoryAsync(farmId, fromDate, toDate);
        return Ok(result);
    }

    // GET: api/admin/farms/{farmId}/tasks/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var farmId = GetCurrentFarmId();
        var result = await _taskService.GetByIdAsync(id, farmId);
        
        if (!result.Success)
            return NotFound(result);
            
        return Ok(result);
    }

    // =============================================
    // CREATE/UPDATE/DELETE ENDPOINTS
    // =============================================

// AgriculturePlatform.API/Controllers/AdminTasksController.cs

private string GetIpAddress()
{
    return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
}

private string GetUserAgent()
{
    return HttpContext.Request.Headers["User-Agent"].ToString() ?? "Unknown";
}

// Update all methods to pass ipAddress and userAgent

[HttpPost]
public async Task<IActionResult> Create([FromBody] CreateTaskDto dto)
{
    var farmId = GetCurrentFarmId();
    var adminId = GetCurrentAdminId();
    
    // ✅ Remove ipAddress and userAgent
    var result = await _taskService.CreateAsync(dto, farmId, adminId);
    
    if (!result.Success)
        return BadRequest(result);
        
    return CreatedAtAction(nameof(GetById), new { farmId, id = result.Data?.Id }, result);
}


// PUT: api/admin/farms/{farmId}/tasks/{id}
[HttpPut("{id}")]
public async Task<IActionResult> Update(int id, [FromBody] UpdateTaskDto dto)
{
    var farmId = GetCurrentFarmId();
    var adminId = GetCurrentAdminId();
    var result = await _taskService.UpdateAsync(id, dto, farmId, adminId);
    
    if (!result.Success)
        return NotFound(result);
        
    return Ok(result);
}

// PUT: api/admin/farms/{farmId}/tasks/{id}/status
[HttpPut("{id}/status")]
public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateTaskStatusDto dto)
{
    var farmId = GetCurrentFarmId();
    var adminId = GetCurrentAdminId();
    var result = await _taskService.UpdateTaskStatusAsync(id, dto.Status, farmId, adminId);
    return Ok(result);
}

// PUT: api/admin/farms/{farmId}/tasks/{id}/reassign
[HttpPut("{id}/reassign")]
public async Task<IActionResult> Reassign(int id, [FromBody] ReassignTaskDto dto)
{
    var farmId = GetCurrentFarmId();
    var adminId = GetCurrentAdminId();
    var result = await _taskService.ReassignTaskAsync(id, dto.NewWorkerId, farmId, adminId);
    return Ok(result);
}

// DELETE: api/admin/farms/{farmId}/tasks/{id}
[HttpDelete("{id}")]
public async Task<IActionResult> Delete(int id)
{
    var farmId = GetCurrentFarmId();
    var adminId = GetCurrentAdminId();
    var result = await _taskService.DeleteAsync(id, farmId, adminId);
    
    if (!result.Success)
        return BadRequest(result);
        
    return Ok(result);
}
    // =============================================
    // TEMPLATE DOWNLOADS
    // =============================================

    // GET: api/admin/farms/{farmId}/tasks/templates/bulk-assign
    [HttpGet("templates/bulk-assign")]
    public async Task<IActionResult> GetBulkAssignTemplate()
    {
        var template = await _excelTaskService.ExportBulkAssignTemplateAsync();
        return File(template, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "bulk_assign_template.xlsx");
    }

    // GET: api/admin/farms/{farmId}/tasks/templates/status-update
    [HttpGet("templates/status-update")]
    public async Task<IActionResult> GetStatusUpdateTemplate()
    {
        var template = await _excelTaskService.ExportTaskStatusUpdateTemplateAsync();
        return File(template, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "bulk_status_update_template.xlsx");
    }

    // GET: api/admin/farms/{farmId}/tasks/templates/reassign
    [HttpGet("templates/reassign")]
    public async Task<IActionResult> GetReassignTemplate()
    {
        var template = await _excelTaskService.ExportTaskReassignTemplateAsync();
        return File(template, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "bulk_reassign_template.xlsx");
    }

    // =============================================
    // EXCEL BULK OPERATIONS
    // =============================================

    // POST: api/admin/farms/{farmId}/tasks/bulk-assign-excel
    [HttpPost("bulk-assign-excel")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<IActionResult> BulkAssignFromExcel(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "Please upload a valid Excel file" });

        var extension = Path.GetExtension(file.FileName).ToLower();
        if (extension != ".xlsx" && extension != ".xls")
            return BadRequest(new { message = "Only Excel files (.xlsx, .xls) are allowed" });

        var farmId = GetCurrentFarmId();
        var adminId = GetCurrentAdminId();
        
        using var stream = file.OpenReadStream();
        var result = await _taskService.BulkAssignTasksFromExcelAsync(stream, farmId, adminId);
        
        return Ok(result);
    }

    // POST: api/admin/farms/{farmId}/tasks/bulk-status-excel
    [HttpPost("bulk-status-excel")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<IActionResult> BulkStatusFromExcel(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "Please upload a valid Excel file" });

        var extension = Path.GetExtension(file.FileName).ToLower();
        if (extension != ".xlsx" && extension != ".xls")
            return BadRequest(new { message = "Only Excel files (.xlsx, .xls) are allowed" });

        var farmId = GetCurrentFarmId();
        var adminId = GetCurrentAdminId();
        
        using var stream = file.OpenReadStream();
        var result = await _taskService.BulkUpdateStatusFromExcelAsync(stream, farmId, adminId);
        
        return Ok(result);
    }

    // POST: api/admin/farms/{farmId}/tasks/bulk-reassign-excel
    [HttpPost("bulk-reassign-excel")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<IActionResult> BulkReassignFromExcel(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "Please upload a valid Excel file" });

        var extension = Path.GetExtension(file.FileName).ToLower();
        if (extension != ".xlsx" && extension != ".xls")
            return BadRequest(new { message = "Only Excel files (.xlsx, .xls) are allowed" });

        var farmId = GetCurrentFarmId();
        var adminId = GetCurrentAdminId();
        
        using var stream = file.OpenReadStream();
        var result = await _taskService.BulkReassignFromExcelAsync(stream, farmId, adminId);
        
        return Ok(result);
    }

    // =============================================
    // JSON BULK OPERATIONS (Alternative to Excel)
    // =============================================

    // POST: api/admin/farms/{farmId}/tasks/bulk-assign
    [HttpPost("bulk-assign")]
    public async Task<IActionResult> BulkAssign([FromBody] BulkAssignTaskDto dto)
    {
        var farmId = GetCurrentFarmId();
        var adminId = GetCurrentAdminId();
        var result = await _taskService.BulkAssignTasksAsync(dto, farmId, adminId);
        
        if (!result.Success)
            return BadRequest(result);
            
        return Ok(result);
    }

    // POST: api/admin/farms/{farmId}/tasks/bulk-status
    [HttpPost("bulk-status")]
    public async Task<IActionResult> BulkUpdateStatus([FromBody] BulkStatusUpdateDto dto)
    {
        var farmId = GetCurrentFarmId();
        var adminId = GetCurrentAdminId();
        var result = await _taskService.BulkUpdateStatusAsync(dto.TaskIds, dto.Status, farmId, adminId);
        
        if (!result.Success)
            return BadRequest(result);
            
        return Ok(result);
    }

    // POST: api/admin/farms/{farmId}/tasks/bulk-reassign
    [HttpPost("bulk-reassign")]
    public async Task<IActionResult> BulkReassign([FromBody] BulkReassignDto dto)
    {
        var farmId = GetCurrentFarmId();
        var adminId = GetCurrentAdminId();
        var result = await _taskService.BulkReassignTasksAsync(dto.TaskIds, dto.NewWorkerId, farmId, adminId);
        
        if (!result.Success)
            return BadRequest(result);
            
        return Ok(result);
    }
}

// =============================================
// ADDITIONAL DTOs
// =============================================

public class BulkStatusUpdateDto
{
    public List<int> TaskIds { get; set; } = new();
    public string Status { get; set; } = string.Empty;
}

public class BulkReassignDto
{
    public List<int> TaskIds { get; set; } = new();
    public int NewWorkerId { get; set; }
}

public class UpdateTaskStatusDto
{
    public string Status { get; set; } = string.Empty;
}

public class ReassignTaskDto
{
    public int NewWorkerId { get; set; }
}