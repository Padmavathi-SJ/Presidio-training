// Application/Interfaces/IYieldReportService.cs
using AgriculturePlatform.Application.Common;
using AgriculturePlatform.Application.DTOs.YieldReport;

namespace AgriculturePlatform.Application.Interfaces;

public interface IYieldReportService
{
    // Worker Operations (Read-only)
    Task<ApiResponse<PagedResult<YieldReportDto>>> GetReportsForWorkerAsync(YieldReportFilterDto filter, int farmId, int workerId);
    Task<ApiResponse<YieldReportDto>> GetReportByIdForWorkerAsync(int id, int farmId, int workerId);
    
    // Admin Operations
    Task<ApiResponse<YieldReportDto>> GenerateReportAsync(GenerateYieldReportDto dto, int farmId, int adminId);
    Task<ApiResponse<YieldReportDto>> CreateScheduledReportAsync(CreateYieldReportDto dto, int farmId, int adminId);
    Task<ApiResponse<PagedResult<YieldReportDto>>> GetAllReportsAsync(YieldReportFilterDto filter, int farmId);
    Task<ApiResponse<YieldReportDto>> GetReportByIdAsync(int id, int farmId);
    Task<ApiResponse<YieldReportDto>> UpdateReportAsync(int id, UpdateYieldReportDto dto, int farmId, int adminId);
    Task<ApiResponse<bool>> DeleteReportAsync(int id, int farmId, int adminId);
    
    // Export operations (returns download link)
    Task<ApiResponse<FileDownloadDto>> ExportReportAsync(int id, string format, int farmId, int adminId);
    
    // Statistics & Analytics
    Task<ApiResponse<YieldComparisonDto>> CompareYieldsAsync(int farmId, int? fieldId, int currentYear, int? previousYear);
    Task<ApiResponse<YieldStatisticsSummaryDto>> GetYieldSummaryAsync(int farmId, DateTime? fromDate, DateTime? toDate);
    
    // Scheduled Reports
    Task ProcessScheduledReportsAsync();
}