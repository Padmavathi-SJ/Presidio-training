// AgriculturePlatform.Application/Services/WorkerProfileService.cs
using System.Security.Cryptography;
using System.Text;
using AutoMapper;
using AgriculturePlatform.Application.Common;
using AgriculturePlatform.Application.DTOs.Worker;
using AgriculturePlatform.Application.Exceptions;
using AgriculturePlatform.Application.Interfaces;
using AgriculturePlatform.Domain.Entities.WorkerManagement;

namespace AgriculturePlatform.Application.Services;

public class WorkerProfileService : IWorkerProfileService
{
    private readonly IWorkerRepository _workerRepository;
    private readonly IAuditLogService _auditLogService;
    private readonly IMapper _mapper;

    public WorkerProfileService(
        IWorkerRepository workerRepository,
        IAuditLogService auditLogService,
        IMapper mapper)
    {
        _workerRepository = workerRepository;
        _auditLogService = auditLogService;
        _mapper = mapper;
    }

    public async Task<ApiResponse<WorkerProfileDto>> GetProfileAsync(int workerId, int farmId)
    {
        var worker = await _workerRepository.GetWorkerWithFarmAsync(workerId, farmId);
        
        if (worker == null)
        {
            return ApiResponse<WorkerProfileDto>.Fail("Worker not found");
        }

        var result = _mapper.Map<WorkerProfileDto>(worker);
        result.FarmName = worker.Farm?.FarmName;
        if (worker.AssignedFields != null)
        {
            result.AssignedFields = worker.AssignedFields
                .Where(af => af.Field != null && !af.IsDeleted)
                .Select(af => new AgriculturePlatform.Application.DTOs.Field.FieldSummaryDto
                {
                    Id = af.Field.Id,
                    FieldName = af.Field.FieldName,
                    AreaHectares = af.Field.AreaHectares,
                    Status = af.Field.Status.ToString()
                })
                .ToList();
        }
        
        return ApiResponse<WorkerProfileDto>.Ok(result);
    }

    public async Task<ApiResponse<WorkerProfileDto>> UpdateProfileAsync(int workerId, int farmId, UpdateWorkerProfileDto dto)
    {
        var worker = await _workerRepository.GetWorkerWithFarmAsync(workerId, farmId);
        
        if (worker == null)
        {
            return ApiResponse<WorkerProfileDto>.Fail("Worker not found");
        }

        var oldWorker = _mapper.Map<Worker>(worker);
        var hasChanges = false;

        // Update name
        if (!string.IsNullOrWhiteSpace(dto.Name) && dto.Name != worker.Name)
        {
            worker.Name = dto.Name;
            hasChanges = true;
        }

        // Update phone
        if (dto.Phone != null && dto.Phone != worker.Phone)
        {
            worker.Phone = dto.Phone;
            hasChanges = true;
        }

        // Handle password change if both current and new password are provided
        if (!string.IsNullOrWhiteSpace(dto.NewPassword))
        {
            if (!VerifyPassword(dto.CurrentPassword, worker.PasswordHash))
            {
                return ApiResponse<WorkerProfileDto>.Fail("Current password is incorrect");
            }
            
            worker.PasswordHash = HashPassword(dto.NewPassword);
            hasChanges = true;
        }

        if (!hasChanges)
        {
            return ApiResponse<WorkerProfileDto>.Fail("No changes to update");
        }

        worker.UpdatedAt = DateTime.UtcNow;
        
        var updated = await _workerRepository.UpdateWorkerProfileAsync(worker);
        
        if (!updated)
        {
            return ApiResponse<WorkerProfileDto>.Fail("Failed to update profile");
        }

        // FIXED: Audit log - correct number of parameters (7)
        await _auditLogService.LogUpdateAsync(
            farmId,           // farmId
            null,             // adminId (null for worker action)
            "Worker",         // entityType
            worker.Id,        // entityId
            oldWorker,        // oldEntity
            worker,           // newEntity
            null,             // ipAddress (optional)
            null);            // userAgent (optional)

        var result = _mapper.Map<WorkerProfileDto>(worker);
        result.FarmName = worker.Farm?.FarmName;
        
        return ApiResponse<WorkerProfileDto>.Ok(result, "Profile updated successfully");
    }

    public async Task<ApiResponse<bool>> ChangePasswordAsync(int workerId, int farmId, ChangeWorkerPasswordDto dto)
    {
        var worker = await _workerRepository.GetByIdAsync(workerId, farmId);
        
        if (worker == null)
        {
            return ApiResponse<bool>.Fail("Worker not found");
        }

        // Verify current password
        if (!VerifyPassword(dto.CurrentPassword, worker.PasswordHash))
        {
            return ApiResponse<bool>.Fail("Current password is incorrect");
        }

        // Update password
        var newPasswordHash = HashPassword(dto.NewPassword);
        var updated = await _workerRepository.UpdateWorkerPasswordAsync(workerId, newPasswordHash);
        
        if (!updated)
        {
            return ApiResponse<bool>.Fail("Failed to update password");
        }

        // FIXED: Use LogAsync instead (8 parameters, but last two are optional)
         await _auditLogService.LogAsync(
        farmId,           // farmId
        null,             // adminId
        workerId,         // workerId
        "CHANGE_PASSWORD",// action
        "Worker",         // entityType
        worker.Id,        // entityId
        null,             // oldValue
        null,             // newValue
        null,             // ipAddress
        null              // userAgent
    );
    
        return ApiResponse<bool>.Ok(true, "Password changed successfully");
    }

    private string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }

    private bool VerifyPassword(string password, string? passwordHash)
    {
        if (string.IsNullOrEmpty(passwordHash)) return false;
        
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        var hashOfInput = Convert.ToBase64String(hashedBytes);
        
        return hashOfInput == passwordHash;
    }
}