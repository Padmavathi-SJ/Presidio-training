// AgriculturePlatform.Application/Services/WorkerService.cs
using AutoMapper;
using AgriculturePlatform.Application.Common;
using AgriculturePlatform.Application.DTOs.Worker;
using AgriculturePlatform.Application.Interfaces;
using AgriculturePlatform.Domain.Entities.WorkerManagement;
using System.Security.Cryptography;
using System.Text;

namespace AgriculturePlatform.Application.Services;

public class WorkerService : IWorkerService
{
    private readonly IWorkerRepository _workerRepository;
    private readonly IAuditLogService _auditLogService;
    private readonly IMapper _mapper;

    public WorkerService(
        IWorkerRepository workerRepository,
        IAuditLogService auditLogService,
        IMapper mapper)
    {
        _workerRepository = workerRepository;
        _auditLogService = auditLogService;
        _mapper = mapper;
    }

    public async Task<ApiResponse<WorkerDto>> CreateAsync(CreateWorkerDto dto, int farmId, int adminId, string ipAddress, string userAgent)
    {
        // Check if email already exists
        if (await _workerRepository.EmailExistsAsync(dto.Email, farmId))
        {
            return ApiResponse<WorkerDto>.Fail($"Worker with email '{dto.Email}' already exists");
        }

        // Create worker
        var worker = _mapper.Map<Worker>(dto);
        worker.FarmId = farmId;
        worker.AdminId = adminId;
        worker.CreatedBy = adminId;
        worker.IsActive = true;
        worker.Role = "Worker";

        var created = await _workerRepository.CreateAsync(worker);

        // Audit log
        await _auditLogService.LogCreateAsync(farmId, adminId, "Worker", created.Id, created, ipAddress, userAgent);

        var result = _mapper.Map<WorkerDto>(created);
        result.Role = "Worker"; 
        return ApiResponse<WorkerDto>.Ok(result, "Worker created successfully");
    }

    public async Task<ApiResponse<WorkerDto>> UpdateAsync(int id, UpdateWorkerDto dto, int farmId, int adminId, string ipAddress, string userAgent)
    {
        var worker = await _workerRepository.GetByIdAsync(id, farmId);
        if (worker == null)
        {
            return ApiResponse<WorkerDto>.Fail($"Worker with ID {id} not found");
        }

        var oldWorker = _mapper.Map<Worker>(worker);

        // Check if email already exists (excluding current worker)
        if (!string.IsNullOrWhiteSpace(dto.Email) && dto.Email != worker.Email)
        {
            if (await _workerRepository.EmailExistsAsync(dto.Email, farmId, id))
            {
                return ApiResponse<WorkerDto>.Fail($"Worker with email '{dto.Email}' already exists");
            }
            worker.Email = dto.Email;
        }

        // Update properties
        if (!string.IsNullOrWhiteSpace(dto.Name)) worker.Name = dto.Name;
        if (!string.IsNullOrWhiteSpace(dto.Phone)) worker.Phone = dto.Phone;
        if (!string.IsNullOrWhiteSpace(dto.Role)) worker.Role = dto.Role?.ToUpper();
        if (dto.IsActive.HasValue) worker.IsActive = dto.IsActive.Value;

        worker.UpdatedAt = DateTime.UtcNow;
        worker.UpdatedBy = adminId;

        await _workerRepository.UpdateAsync(worker);

        // Audit log
        await _auditLogService.LogUpdateAsync(farmId, adminId, "Worker", worker.Id, oldWorker, worker, ipAddress, userAgent);

        var result = _mapper.Map<WorkerDto>(worker);
        return ApiResponse<WorkerDto>.Ok(result, "Worker updated successfully");
    }

    public async Task<ApiResponse<bool>> SoftDeleteAsync(int id, int farmId, int adminId, string ipAddress, string userAgent)
    {
        var worker = await _workerRepository.GetByIdAsync(id, farmId, true);
        if (worker == null)
        {
            return ApiResponse<bool>.Fail($"Worker with ID {id} not found");
        }

        if (worker.IsDeleted)
        {
            return ApiResponse<bool>.Fail($"Worker with ID {id} is already deleted");
        }

        await _workerRepository.SoftDeleteAsync(worker, adminId);

        await _auditLogService.LogSoftDeleteAsync(farmId, adminId, "Worker", worker.Id, worker, ipAddress, userAgent);

        return ApiResponse<bool>.Ok(true, "Worker deleted successfully");
    }

    public async Task<ApiResponse<WorkerDto>> GetByIdAsync(int id, int farmId)
    {
        var worker = await _workerRepository.GetByIdAsync(id, farmId);
        if (worker == null)
        {
            return ApiResponse<WorkerDto>.Fail($"Worker with ID {id} not found");
        }

        var result = _mapper.Map<WorkerDto>(worker);
        result.LastLoginDaysAgo = await GetLastLoginDaysAsync(worker.Id);
        
        return ApiResponse<WorkerDto>.Ok(result);
    }

    public async Task<ApiResponse<PagedResult<WorkerDto>>> GetAllAsync(WorkerFilterDto filter, int farmId)
    {
        var paginationParams = new PaginationParams
        {
            Page = filter.Page ?? 1,
            PageSize = filter.PageSize ?? 10,
            SortBy = filter.SortBy,
            IsDescending = filter.IsDescending
        };

        var pagedResult = await _workerRepository.GetPagedAsync(
            farmId,
            filter.Name,
            filter.Email,
            filter.Role,
            filter.IsActive,
            filter.HireDateFrom,
            filter.HireDateTo,
            filter.IncludeDeleted ?? false,
            paginationParams);

        var dtos = _mapper.Map<List<WorkerDto>>(pagedResult.Items);
        
        // Get last login days for each worker
        foreach (var dto in dtos)
        {
            dto.LastLoginDaysAgo = await GetLastLoginDaysAsync(dto.Id);
        }

        var result = new PagedResult<WorkerDto>
        {
            Items = dtos,
            TotalCount = pagedResult.TotalCount,
            Page = pagedResult.Page,
            PageSize = pagedResult.PageSize
        };

        return ApiResponse<PagedResult<WorkerDto>>.Ok(result);
    }

    public async Task<ApiResponse<bool>> ActivateAsync(int id, int farmId, int adminId, string ipAddress, string userAgent)
    {
        var worker = await _workerRepository.GetByIdAsync(id, farmId);
        if (worker == null)
        {
            return ApiResponse<bool>.Fail($"Worker with ID {id} not found");
        }

        if (worker.IsActive)
        {
            return ApiResponse<bool>.Fail($"Worker with ID {id} is already active");
        }

        worker.IsActive = true;
        worker.UpdatedAt = DateTime.UtcNow;
        worker.UpdatedBy = adminId;

        await _workerRepository.UpdateAsync(worker);

        await _auditLogService.LogAsync(farmId, adminId, worker.Id, "ACTIVATE", "Worker", worker.Id, null, worker, ipAddress, userAgent);

        return ApiResponse<bool>.Ok(true, "Worker activated successfully");
    }

    public async Task<ApiResponse<bool>> DeactivateAsync(int id, int farmId, int adminId, string ipAddress, string userAgent)
    {
        var worker = await _workerRepository.GetByIdAsync(id, farmId);
        if (worker == null)
        {
            return ApiResponse<bool>.Fail($"Worker with ID {id} not found");
        }

        if (!worker.IsActive)
        {
            return ApiResponse<bool>.Fail($"Worker with ID {id} is already inactive");
        }

        worker.IsActive = false;
        worker.UpdatedAt = DateTime.UtcNow;
        worker.UpdatedBy = adminId;

        await _workerRepository.UpdateAsync(worker);

        await _auditLogService.LogAsync(farmId, adminId, worker.Id, "DEACTIVATE", "Worker", worker.Id, null, worker, ipAddress, userAgent);

        return ApiResponse<bool>.Ok(true, "Worker deactivated successfully");
    }

    public async Task<ApiResponse<bool>> ResetPasswordAsync(int id, int farmId, int adminId, string newPassword, string ipAddress, string userAgent)
    {
        var worker = await _workerRepository.GetByIdAsync(id, farmId);
        if (worker == null)
        {
            return ApiResponse<bool>.Fail($"Worker with ID {id} not found");
        }

        var passwordHash = HashPassword(newPassword);
        await _workerRepository.UpdatePasswordAsync(id, passwordHash);

        await _auditLogService.LogAsync(farmId, adminId, worker.Id, "RESET_PASSWORD", "Worker", worker.Id, null, new { PasswordReset = true }, ipAddress, userAgent);

        return ApiResponse<bool>.Ok(true, "Password reset successfully");
    }

    public async Task<ApiResponse<WorkerLoginHistoryDto>> GetLoginHistoryAsync(int id, int farmId)
    {
        var worker = await _workerRepository.GetByIdAsync(id, farmId);
        if (worker == null)
        {
            return ApiResponse<WorkerLoginHistoryDto>.Fail($"Worker with ID {id} not found");
        }

        var lastLogin = await _workerRepository.GetLastLoginAsync(id);
        
        // Count total logins
        var totalLogins = 0; // You can implement this in repository

        var result = new WorkerLoginHistoryDto
        {
            WorkerId = worker.Id,
            WorkerName = worker.Name,
            LastLoginAt = lastLogin,
            TotalLogins = totalLogins
        };

        return ApiResponse<WorkerLoginHistoryDto>.Ok(result);
    }

    public async Task<bool> ValidateWorkerOwnershipAsync(int workerId, int farmId)
    {
        return await _workerRepository.ExistsAsync(workerId, farmId);
    }

    private async Task<int?> GetLastLoginDaysAsync(int workerId)
    {
        var lastLogin = await _workerRepository.GetLastLoginAsync(workerId);
        if (!lastLogin.HasValue) return null;
        
        var days = (DateTime.UtcNow - lastLogin.Value).Days;
        return days;
    }

    private string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }
}