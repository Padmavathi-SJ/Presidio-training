// AgriculturePlatform.Application/Interfaces/IWorkerFieldAssignmentService.cs
using AgriculturePlatform.Application.Common;
using AgriculturePlatform.Application.DTOs.Worker;
using AgriculturePlatform.Application.DTOs.WorkerField;

namespace AgriculturePlatform.Application.Interfaces;

public interface IWorkerFieldAssignmentService
{
    // Admin operations
    Task<ApiResponse<WorkerFieldAssignmentDto>> AssignFieldToWorkerAsync(AssignFieldToWorkerDto dto, int farmId, int adminId, string ipAddress, string userAgent);
    Task<ApiResponse<WorkerFieldAssignmentDto>> UpdateAssignmentAsync(int id, AssignFieldToWorkerDto dto, int farmId, int adminId, string ipAddress, string userAgent);
    Task<ApiResponse<bool>> RemoveAssignmentAsync(int id, int farmId, int adminId, string ipAddress, string userAgent);
    Task<ApiResponse<PagedResult<WorkerFieldAssignmentDto>>> GetAllAssignmentsAsync(WorkerFieldFilterDto filter, int farmId);
    
    // Worker operations - Use fully qualified name with the full namespace
    Task<ApiResponse<List<AgriculturePlatform.Application.DTOs.Worker.WorkerFieldDetailDto>>> GetMyAssignedFieldsAsync(int workerId, int farmId);
}