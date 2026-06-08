// AgriculturePlatform.Application/Interfaces/IExcelTaskService.cs
using AgriculturePlatform.Application.DTOs.Task;

namespace AgriculturePlatform.Application.Interfaces;

public interface IExcelTaskService
{
    // Read operations
    Task<List<BulkAssignTaskExcelDto>> ReadBulkAssignTasksFromExcelAsync(Stream fileStream);
    Task<List<BulkStatusUpdateExcelDto>> ReadBulkStatusUpdateFromExcelAsync(Stream fileStream);
    Task<List<BulkReassignExcelDto>> ReadBulkReassignFromExcelAsync(Stream fileStream);
    
    // Template exports
    Task<byte[]> ExportBulkAssignTemplateAsync();
    Task<byte[]> ExportTaskStatusUpdateTemplateAsync();
    Task<byte[]> ExportTaskReassignTemplateAsync();
}