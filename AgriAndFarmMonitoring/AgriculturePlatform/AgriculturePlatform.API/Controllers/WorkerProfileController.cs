// AgriculturePlatform.API/Controllers/WorkerProfileController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using AgriculturePlatform.Application.DTOs.Worker;
using AgriculturePlatform.Application.Interfaces;
using AgriculturePlatform.API.Filters;

namespace AgriculturePlatform.API.Controllers;

[ApiController]
[Route("api/worker/profile")]
[Authorize]
[AuthorizeWorkerFarm]
public class WorkerProfileController : ControllerBase
{
    private readonly IWorkerProfileService _workerProfileService;

    public WorkerProfileController(IWorkerProfileService workerProfileService)
    {
        _workerProfileService = workerProfileService;
    }

    private int GetCurrentWorkerId()
    {
        var workerIdClaim = User.FindFirst("workerId")?.Value 
                         ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(workerIdClaim, out var id) ? id : 0;
    }

    private int GetCurrentFarmId()
    {
        return int.Parse(User.FindFirst("farmId")?.Value ?? "0");
    }

    // GET: api/worker/profile
    [HttpGet]
    public async Task<IActionResult> GetProfile()
    {
        var workerId = GetCurrentWorkerId();
        var farmId = GetCurrentFarmId();
        
        var result = await _workerProfileService.GetProfileAsync(workerId, farmId);
        
        if (!result.Success)
            return NotFound(result);
            
        return Ok(result);
    }

    // PUT: api/worker/profile
    [HttpPut]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateWorkerProfileDto dto)
    {
        var workerId = GetCurrentWorkerId();
        var farmId = GetCurrentFarmId();
        
        var result = await _workerProfileService.UpdateProfileAsync(workerId, farmId, dto);
        
        if (!result.Success)
            return BadRequest(result);
            
        return Ok(result);
    }

    // PUT: api/worker/profile/change-password
    [HttpPut("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangeWorkerPasswordDto dto)
    {
        var workerId = GetCurrentWorkerId();
        var farmId = GetCurrentFarmId();
        
        var result = await _workerProfileService.ChangePasswordAsync(workerId, farmId, dto);
        
        if (!result.Success)
            return BadRequest(result);
            
        return Ok(result);
    }
}