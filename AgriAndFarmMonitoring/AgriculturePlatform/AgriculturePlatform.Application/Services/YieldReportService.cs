// Application/Services/YieldReportService.cs
using AutoMapper;
using AgriculturePlatform.Application.Common;
using AgriculturePlatform.Application.DTOs.YieldReport;
using AgriculturePlatform.Application.Interfaces;
using AgriculturePlatform.Domain.Entities.YieldReports;
using AgriculturePlatform.Domain.Entities.AdminEntities;
using AgriculturePlatform.Domain.Entities.CropMonitoring;
using AgriculturePlatform.Domain.Entities.WorkerManagement;
using AgriculturePlatform.Domain.Enums;
using System.Text.Json;

namespace AgriculturePlatform.Application.Services;

public class YieldReportService : IYieldReportService
{
    private readonly IYieldReportRepository _reportRepository;
    private readonly IHarvestRepository _harvestRepository;
    private readonly IFieldRepository _fieldRepository;
    private readonly ICropCycleRepository _cropCycleRepository;
    private readonly IWorkerFieldAssignmentRepository _assignmentRepository;
    private readonly IAuditLogService _auditLogService;
    private readonly IFarmRepository _farmRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly IMapper _mapper;

    public YieldReportService(
        IYieldReportRepository reportRepository,
        IHarvestRepository harvestRepository,
        IFieldRepository fieldRepository,
        ICropCycleRepository cropCycleRepository,
        IWorkerFieldAssignmentRepository assignmentRepository,
        IAuditLogService auditLogService,
        IFarmRepository farmRepository,
        IFileStorageService fileStorageService,
        IMapper mapper)
    {
        _reportRepository = reportRepository;
        _harvestRepository = harvestRepository;
        _fieldRepository = fieldRepository;
        _cropCycleRepository = cropCycleRepository;
        _assignmentRepository = assignmentRepository;
        _auditLogService = auditLogService;
        _farmRepository = farmRepository;
        _fileStorageService = fileStorageService;
        _mapper = mapper;
    }

    // =============================================
    // WORKER OPERATIONS (Read-only)
    // =============================================

    public async Task<ApiResponse<PagedResult<YieldReportDto>>> GetReportsForWorkerAsync(
        YieldReportFilterDto filter, int farmId, int workerId)
    {
        // Get fields assigned to worker
        var assignedFields = await _assignmentRepository.GetFieldsByWorkerAsync(workerId, farmId);
        var fieldIds = assignedFields.Select(f => f.Id).ToList();

        var paginationParams = new PaginationParams
        {
            Page = filter.Page ?? 1,
            PageSize = filter.PageSize ?? 20,
            SortBy = filter.SortBy,
            IsDescending = filter.IsDescending
        };

        var pagedResult = await _reportRepository.GetPagedAsync(
            farmId,
            filter.CropCycleId,
            null,
            filter.ReportType,
            filter.FromDate,
            filter.ToDate,
            filter.IsScheduled,
            paginationParams);

        // Filter reports by assigned fields
        var filteredItems = pagedResult.Items.Where(r => !r.FieldId.HasValue || fieldIds.Contains(r.FieldId.Value)).ToList();

        var dtos = _mapper.Map<List<YieldReportDto>>(filteredItems);
        
        var result = new PagedResult<YieldReportDto>
        {
            Items = dtos,
            TotalCount = filteredItems.Count,
            Page = pagedResult.Page,
            PageSize = pagedResult.PageSize
        };

        return ApiResponse<PagedResult<YieldReportDto>>.Ok(result);
    }

    public async Task<ApiResponse<YieldReportDto>> GetReportByIdForWorkerAsync(int id, int farmId, int workerId)
    {
        var report = await _reportRepository.GetByIdAsync(id, farmId);
        if (report == null)
            return ApiResponse<YieldReportDto>.Fail($"Report with ID {id} not found");

        if (report.FieldId.HasValue)
        {
            var hasAccess = await _assignmentRepository.HasWorkerAccessToFieldAsync(workerId, report.FieldId.Value, farmId);
            if (!hasAccess)
                return ApiResponse<YieldReportDto>.Fail("You don't have permission to view this report");
        }

        var result = _mapper.Map<YieldReportDto>(report);
        return ApiResponse<YieldReportDto>.Ok(result);
    }

    // =============================================
    // ADMIN OPERATIONS
    // =============================================

    public async Task<ApiResponse<YieldReportDto>> GenerateReportAsync(GenerateYieldReportDto dto, int farmId, int adminId)
    {
        if (dto.StartDate > dto.EndDate)
            return ApiResponse<YieldReportDto>.Fail("Start date must be less than end date");

        var report = new YieldReport
        {
            FarmId = farmId,
            AdminId = adminId,
            CropCycleId = dto.CropCycleId,
            FieldId = dto.FieldId,
            ReportName = string.IsNullOrWhiteSpace(dto.ReportName) 
                ? $"Yield Report {dto.StartDate:yyyy-MM-dd} to {dto.EndDate:yyyy-MM-dd}" 
                : dto.ReportName,
            ReportType = "CUSTOM",
            StartDate = dto.StartDate.ToUniversalTime(),
            EndDate = dto.EndDate.ToUniversalTime(),
            CreatedBy = adminId,
            CreatedAt = DateTime.UtcNow
        };

        await CalculateReportStatistics(report, farmId);
        
        var created = await _reportRepository.CreateAsync(report);
        
        await _auditLogService.LogCreateAsync(farmId, adminId, "YieldReport", created.Id, created, null, null);

        var result = _mapper.Map<YieldReportDto>(created);
        return ApiResponse<YieldReportDto>.Ok(result, "Report generated successfully");
    }

    public async Task<ApiResponse<YieldReportDto>> CreateScheduledReportAsync(CreateYieldReportDto dto, int farmId, int adminId)
    {
        var report = new YieldReport
        {
            FarmId = farmId,
            AdminId = adminId,
            CropCycleId = dto.CropCycleId,
            FieldId = dto.FieldId,
            ReportName = dto.ReportName,
            ReportType = dto.ReportType,
            StartDate = dto.StartDate.ToUniversalTime(),
            EndDate = dto.EndDate.ToUniversalTime(),
            IsScheduled = true,
            ScheduleCron = dto.ScheduleCron,
            CreatedBy = adminId,
            CreatedAt = DateTime.UtcNow
        };

        report.NextScheduledRun = CalculateNextRunTime(dto.ScheduleCron);

        var created = await _reportRepository.CreateAsync(report);
        
        await _auditLogService.LogCreateAsync(farmId, adminId, "YieldReport", created.Id, created, null, null);

        var result = _mapper.Map<YieldReportDto>(created);
        return ApiResponse<YieldReportDto>.Ok(result, "Scheduled report created successfully");
    }

    public async Task<ApiResponse<PagedResult<YieldReportDto>>> GetAllReportsAsync(YieldReportFilterDto filter, int farmId)
    {
        var paginationParams = new PaginationParams
        {
            Page = filter.Page ?? 1,
            PageSize = filter.PageSize ?? 20,
            SortBy = filter.SortBy,
            IsDescending = filter.IsDescending
        };

        var pagedResult = await _reportRepository.GetPagedAsync(
            farmId,
            filter.CropCycleId,
            filter.FieldId,
            filter.ReportType,
            filter.FromDate,
            filter.ToDate,
            filter.IsScheduled,
            paginationParams);

        var dtos = _mapper.Map<List<YieldReportDto>>(pagedResult.Items);
        
        var result = new PagedResult<YieldReportDto>
        {
            Items = dtos,
            TotalCount = pagedResult.TotalCount,
            Page = pagedResult.Page,
            PageSize = pagedResult.PageSize
        };

        return ApiResponse<PagedResult<YieldReportDto>>.Ok(result);
    }

    public async Task<ApiResponse<YieldReportDto>> GetReportByIdAsync(int id, int farmId)
    {
        var report = await _reportRepository.GetByIdAsync(id, farmId);
        if (report == null)
            return ApiResponse<YieldReportDto>.Fail($"Report with ID {id} not found");

        var result = _mapper.Map<YieldReportDto>(report);
        return ApiResponse<YieldReportDto>.Ok(result);
    }

    public async Task<ApiResponse<YieldReportDto>> UpdateReportAsync(int id, UpdateYieldReportDto dto, int farmId, int adminId)
    {
        var report = await _reportRepository.GetByIdAsync(id, farmId);
        if (report == null)
            return ApiResponse<YieldReportDto>.Fail($"Report with ID {id} not found");

        var oldReport = _mapper.Map<YieldReport>(report);

        if (!string.IsNullOrWhiteSpace(dto.ReportName))
            report.ReportName = dto.ReportName;
        if (dto.IsScheduled.HasValue)
            report.IsScheduled = dto.IsScheduled.Value;
        if (!string.IsNullOrWhiteSpace(dto.ScheduleCron))
        {
            report.ScheduleCron = dto.ScheduleCron;
            report.NextScheduledRun = CalculateNextRunTime(dto.ScheduleCron);
        }

        report.UpdatedAt = DateTime.UtcNow;
        report.UpdatedBy = adminId;

        await _reportRepository.UpdateAsync(report);
        
        await _auditLogService.LogUpdateAsync(farmId, adminId, "YieldReport", report.Id, oldReport, report, null, null);

        var result = _mapper.Map<YieldReportDto>(report);
        return ApiResponse<YieldReportDto>.Ok(result, "Report updated successfully");
    }

    public async Task<ApiResponse<bool>> DeleteReportAsync(int id, int farmId, int adminId)
    {
        var report = await _reportRepository.GetByIdAsync(id, farmId);
        if (report == null)
            return ApiResponse<bool>.Fail($"Report with ID {id} not found");

        // Also delete the physical file if it exists
        if (!string.IsNullOrEmpty(report.FilePath))
        {
            await _fileStorageService.DeleteFileAsync(report.FilePath);
        }

        await _reportRepository.DeleteAsync(report);
        
        await _auditLogService.LogSoftDeleteAsync(farmId, adminId, "YieldReport", report.Id, report, null, null);

        return ApiResponse<bool>.Ok(true, "Report deleted successfully");
    }

public async Task<ApiResponse<FileDownloadDto>> ExportReportAsync(int id, string format, int farmId, int adminId)
{
    var report = await _reportRepository.GetByIdAsync(id, farmId);
    if (report == null)
        return ApiResponse<FileDownloadDto>.Fail($"Report with ID {id} not found");

    byte[] fileContent;
    string fileExtension;
    
    switch (format.ToUpper())
    {
        case "CSV":
            fileContent = await ExportToCsvBytes(report);  // No farmId needed - uses report data
            fileExtension = "csv";
            break;
        case "JSON":
            fileContent = await ExportToJsonBytes(report);
            fileExtension = "json";
            break;
        default:
            return ApiResponse<FileDownloadDto>.Fail($"Format {format} not supported. Supported formats: CSV, JSON");
    }
    
    // Generate unique filename
    var fileName = $"yield_report_{report.Id}_{DateTime.Now:yyyyMMdd_HHmmss}.{fileExtension}";
    
    // Save file using file storage service
    var relativePath = await _fileStorageService.SaveFileAsync(fileContent, fileName, "");
    
    // Update report with file information
    report.FileName = fileName;
    report.FilePath = relativePath;
    report.FileFormat = format.ToUpper();
    report.FileSize = fileContent.Length;
    report.ExportedAt = DateTime.UtcNow;
    report.ExportedBy = adminId;
    await _reportRepository.UpdateAsync(report);
    
    // Generate download URL
    var downloadUrl = _fileStorageService.GetDownloadUrl(fileName);
    
    var result = new FileDownloadDto
    {
        ReportId = report.Id,
        ReportName = report.ReportName,
        FileName = fileName,
        DownloadUrl = downloadUrl,
        FileFormat = format.ToUpper(),
        FileSize = fileContent.Length,
        ExportedAt = DateTime.UtcNow
    };
    
    await _auditLogService.LogCreateAsync(farmId, adminId, "YieldReportExport", report.Id, 
        new { FileName = fileName, Format = format }, null, null);
    
    return ApiResponse<FileDownloadDto>.Ok(result, $"Report exported as {format}");
}

    // =============================================
    // STATISTICS & ANALYTICS
    // =============================================

    public async Task<ApiResponse<YieldComparisonDto>> CompareYieldsAsync(int farmId, int? fieldId, int currentYear, int? previousYear)
    {
        var prevYear = previousYear ?? currentYear - 1;
        var comparison = new YieldComparisonDto();

        var fields = await _fieldRepository.GetByFarmIdAsync(farmId);
        foreach (var field in fields)
        {
            var currentYield = await _reportRepository.GetTotalYieldForPeriodAsync(
                farmId, new DateTime(currentYear, 1, 1), new DateTime(currentYear, 12, 31), field.Id);
            var previousYield = await _reportRepository.GetTotalYieldForPeriodAsync(
                farmId, new DateTime(prevYear, 1, 1), new DateTime(prevYear, 12, 31), field.Id);

            var changePercent = previousYield > 0 
                ? ((currentYield - previousYield) / previousYield) * 100 
                : 0;

            comparison.FieldComparisons.Add(new FieldComparisonDto
            {
                FieldName = field.FieldName,
                CurrentYield = currentYield,
                PreviousYield = previousYield,
                ChangePercentage = Math.Round(changePercent, 2),
                Trend = changePercent > 5 ? "INCREASING" : changePercent < -5 ? "DECREASING" : "STABLE"
            });
        }

        var bestField = comparison.FieldComparisons.OrderByDescending(f => f.CurrentYield).FirstOrDefault();
        var worstField = comparison.FieldComparisons.OrderBy(f => f.CurrentYield).FirstOrDefault();

        comparison.Summary = new ComparisonSummaryDto
        {
            BestPerformingField = bestField?.FieldName ?? "N/A",
            BestYield = bestField?.CurrentYield ?? 0,
            WorstPerformingField = worstField?.FieldName ?? "N/A",
            WorstYield = worstField?.CurrentYield ?? 0,
            Recommendation = GenerateRecommendation(comparison.FieldComparisons)
        };

        return ApiResponse<YieldComparisonDto>.Ok(comparison);
    }

    public async Task<ApiResponse<YieldStatisticsSummaryDto>> GetYieldSummaryAsync(int farmId, DateTime? fromDate, DateTime? toDate)
    {
        var endDate = toDate ?? DateTime.UtcNow;
        var startDate = fromDate ?? endDate.AddDays(-30);

        var totalYield = await _reportRepository.GetTotalYieldForPeriodAsync(farmId, startDate, endDate);
        
        var harvests = await _harvestRepository.GetByDateRangeAsync(farmId, startDate, endDate);
        var approvedHarvests = harvests.Where(h => h.ApprovalStatus == "APPROVED").ToList();

        var summary = new YieldStatisticsSummaryDto
        {
            TotalYieldKg = totalYield,
            TotalHarvests = approvedHarvests.Count,
            AverageYieldPerHarvest = approvedHarvests.Any() ? totalYield / approvedHarvests.Count : 0,
            TotalValue = approvedHarvests.Sum(h => h.TotalValue ?? 0),
            StartDate = startDate,
            EndDate = endDate
        };

        return ApiResponse<YieldStatisticsSummaryDto>.Ok(summary);
    }

    public async Task ProcessScheduledReportsAsync()
    {
        var now = DateTime.UtcNow;
        var farms = await _farmRepository.GetAllActiveFarmsAsync();
        
        foreach (var farm in farms)
        {
            var scheduledReports = await _reportRepository.GetScheduledReportsAsync(farm.Id);
            var reportsToRun = scheduledReports.Where(r => r.NextScheduledRun <= now).ToList();

            foreach (var report in reportsToRun)
            {
                await CalculateReportStatistics(report, farm.Id);
                report.LastGeneratedAt = now;
                report.NextScheduledRun = CalculateNextRunTime(report.ScheduleCron);
                await _reportRepository.UpdateAsync(report);
            }
        }
    }

    // =============================================
    // PRIVATE HELPER METHODS
    // =============================================

    private async Task CalculateReportStatistics(YieldReport report, int farmId)
    {
        var harvests = await _harvestRepository.GetByDateRangeAsync(farmId, report.StartDate, report.EndDate);
        var approvedHarvests = harvests.Where(h => h.ApprovalStatus == "APPROVED").ToList();

        if (report.FieldId.HasValue)
            approvedHarvests = approvedHarvests.Where(h => h.FieldId == report.FieldId.Value).ToList();

        if (report.CropCycleId.HasValue)
            approvedHarvests = approvedHarvests.Where(h => h.CropCycleId == report.CropCycleId.Value).ToList();

        // Basic Statistics
        report.TotalHarvests = approvedHarvests.Count;
        report.TotalYieldKg = approvedHarvests.Sum(h => h.QuantityKg);
        report.TotalValue = approvedHarvests.Sum(h => h.TotalValue ?? 0);
        report.AveragePricePerKg = approvedHarvests.Any() && approvedHarvests.Sum(h => h.QuantityKg) > 0
            ? report.TotalValue / report.TotalYieldKg
            : 0;

        // Yield per hectare
        var fieldsWithArea = approvedHarvests.Where(h => h.Field != null && h.Field.AreaHectares > 0).ToList();
        if (fieldsWithArea.Any())
        {
            var totalArea = fieldsWithArea.Select(h => h.Field!.AreaHectares ?? 0).Distinct().Sum();
            report.AverageYieldPerHectare = totalArea > 0 ? report.TotalYieldKg / totalArea : 0;
        }

        // Quality Statistics
        var qualityChecks = approvedHarvests.SelectMany(h => h.QualityChecks ?? new List<QualityCheck>()).ToList();
        var approvedChecks = qualityChecks.Where(q => q.ApprovalStatus == "APPROVED").ToList();
        
        if (approvedChecks.Any())
        {
            var passedChecks = approvedChecks.Count(q => q.FinalGrade != QualityGradeEnum.REJECTED && q.FinalGrade != QualityGradeEnum.D);
            var rejectedChecks = approvedChecks.Count(q => q.FinalGrade == QualityGradeEnum.REJECTED);
            
            report.PassRate = ((decimal)passedChecks / approvedChecks.Count) * 100m;
            report.RejectionRate = ((decimal)rejectedChecks / approvedChecks.Count) * 100m;
            
            var gradeValues = approvedChecks.Select(q => GetGradeValue(q.FinalGrade));
            var avgGrade = gradeValues.Average();
            report.AverageQualityGrade = GetGradeFromValue((decimal)avgGrade);
        }
        else
        {
            report.PassRate = 0;
            report.RejectionRate = 0;
            report.AverageQualityGrade = "N/A";
        }

        // Field Breakdown
        var fieldBreakdown = approvedHarvests
            .Where(h => h.Field != null)
            .GroupBy(h => h.Field!.FieldName)
            .Select(g => 
            {
                var firstField = g.First().Field;
                var areaHectares = firstField?.AreaHectares ?? 0;
                var totalYield = g.Sum(h => h.QuantityKg);
                
                return new FieldYieldBreakdownDto
                {
                    FieldName = g.Key,
                    TotalYieldKg = totalYield,
                    HarvestCount = g.Count(),
                    YieldPerHectare = areaHectares > 0 ? totalYield / areaHectares : 0,
                    PercentageOfTotal = report.TotalYieldKg > 0 ? (totalYield / report.TotalYieldKg) * 100 : 0
                };
            })
            .OrderByDescending(f => f.TotalYieldKg)
            .ToList();
        report.FieldBreakdownJson = JsonSerializer.Serialize(fieldBreakdown);

        // Crop Type Breakdown
        var cropBreakdown = approvedHarvests
            .Where(h => h.CropCycle != null && h.CropCycle.CropType != null)
            .GroupBy(h => h.CropCycle!.CropType!.ToString())
            .Select(g => new CropTypeYieldBreakdownDto
            {
                CropType = g.Key,
                TotalYieldKg = g.Sum(h => h.QuantityKg),
                HarvestCount = g.Count(),
                AveragePricePerKg = g.Average(h => h.PricePerKg ?? 0),
                TotalValue = g.Sum(h => h.TotalValue ?? 0),
                PercentageOfTotal = report.TotalYieldKg > 0 ? (g.Sum(h => h.QuantityKg) / report.TotalYieldKg) * 100 : 0
            })
            .OrderByDescending(c => c.TotalYieldKg)
            .ToList();
        report.CropTypeBreakdownJson = JsonSerializer.Serialize(cropBreakdown);

        // Monthly Trend
        var monthlyTrend = approvedHarvests
            .GroupBy(h => new { h.HarvestDate.Year, h.HarvestDate.Month })
            .Select(g => new MonthlyYieldTrendDto
            {
                Year = g.Key.Year,
                Month = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM"),
                YieldKg = g.Sum(h => h.QuantityKg),
                HarvestCount = g.Count(),
                AveragePrice = g.Average(h => h.PricePerKg ?? 0),
                TotalValue = g.Sum(h => h.TotalValue ?? 0)
            })
            .OrderBy(m => m.Year)
            .ThenBy(m => m.Month)
            .ToList();
        report.MonthlyTrendJson = JsonSerializer.Serialize(monthlyTrend);

        // Quality Distribution
        if (approvedChecks.Any())
        {
            var qualityDist = approvedChecks
                .Where(q => q.FinalGrade.HasValue)
                .GroupBy(q => q.FinalGrade!.Value.ToString())
                .Select(g => new QualityDistributionDto
                {
                    Grade = g.Key,
                    Count = g.Count(),
                    Percentage = (decimal)g.Count() / approvedChecks.Count * 100
                })
                .OrderByDescending(q => q.Grade)
                .ToList();
            report.QualityDistributionJson = JsonSerializer.Serialize(qualityDist);
        }
    }

    private DateTime CalculateNextRunTime(string? cronExpression)
    {
        if (string.IsNullOrEmpty(cronExpression))
            return DateTime.UtcNow.AddDays(7);

        return cronExpression switch
        {
            "0 0 * * *" => DateTime.UtcNow.Date.AddDays(1),
            "0 0 * * 0" => DateTime.UtcNow.Date.AddDays(7 - (int)DateTime.UtcNow.DayOfWeek),
            "0 0 1 * *" => new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1).AddMonths(1),
            _ => DateTime.UtcNow.AddDays(7)
        };
    }

    private int GetGradeValue(QualityGradeEnum? grade)
    {
        return grade switch
        {
            QualityGradeEnum.A_PLUS => 5,
            QualityGradeEnum.A => 4,
            QualityGradeEnum.B => 3,
            QualityGradeEnum.C => 2,
            QualityGradeEnum.D => 1,
            QualityGradeEnum.REJECTED => 0,
            _ => 0
        };
    }

    private string GetGradeFromValue(decimal value)
    {
        return value switch
        {
            >= 4.5m => "A_PLUS",
            >= 3.5m => "A",
            >= 2.5m => "B",
            >= 1.5m => "C",
            >= 0.5m => "D",
            _ => "REJECTED"
        };
    }

    private string GenerateRecommendation(List<FieldComparisonDto> comparisons)
    {
        var increasing = comparisons.Count(c => c.Trend == "INCREASING");
        var decreasing = comparisons.Count(c => c.Trend == "DECREASING");
        var total = comparisons.Count;

        if (decreasing > total / 2)
            return "Multiple fields show decreasing yields. Consider reviewing soil health, irrigation, and fertilizer practices.";
        
        if (increasing > total / 2)
            return "Overall yield trends are positive. Continue current practices and monitor for any changes.";
        
        return "Yield trends are mixed. Analyze best and worst performing fields to identify improvement opportunities.";
    }

// Application/Services/YieldReportService.cs - Replace ExportToCsvBytes method

private async Task<byte[]> ExportToCsvBytes(YieldReport report)
{
    var csv = new System.Text.StringBuilder();
    
    // =============================================
    // SECTION 1: REPORT HEADER
    // =============================================
    csv.AppendLine("\"Report Name\",\"Value\"");
    csv.AppendLine($"\"Report Name\",\"{EscapeCsvValue(report.ReportName)}\"");
    csv.AppendLine($"\"Generated On\",\"{DateTime.Now:yyyy-MM-dd HH:mm:ss}\"");
    csv.AppendLine($"\"Date Range Start\",\"{report.StartDate:yyyy-MM-dd}\"");
    csv.AppendLine($"\"Date Range End\",\"{report.EndDate:yyyy-MM-dd}\"");
    csv.AppendLine();
    
    // =============================================
    // SECTION 2: SUMMARY STATISTICS
    // =============================================
    csv.AppendLine("\"Metric\",\"Value\"");
    csv.AppendLine($"\"Total Yield (kg)\",\"{report.TotalYieldKg:N0}\"");
    csv.AppendLine($"\"Total Harvests\",\"{report.TotalHarvests}\"");
    csv.AppendLine($"\"Average Price per kg\",\"{report.AveragePricePerKg:F2}\"");
    csv.AppendLine($"\"Total Value\",\"{report.TotalValue:C}\"");
    csv.AppendLine($"\"Yield per Hectare (kg/ha)\",\"{report.AverageYieldPerHectare:N0}\"");
    csv.AppendLine($"\"Pass Rate (%)\",\"{report.PassRate:F1}\"");
    csv.AppendLine($"\"Rejection Rate (%)\",\"{report.RejectionRate:F1}\"");
    csv.AppendLine($"\"Average Quality Grade\",\"{report.AverageQualityGrade}\"");
    csv.AppendLine();
    
    // =============================================
    // SECTION 3: FIELD BREAKDOWN
    // =============================================
    if (!string.IsNullOrEmpty(report.FieldBreakdownJson))
    {
        var fieldBreakdown = JsonSerializer.Deserialize<List<FieldYieldBreakdownDto>>(report.FieldBreakdownJson);
        if (fieldBreakdown != null && fieldBreakdown.Any())
        {
            csv.AppendLine("\"=== FIELD BREAKDOWN ===\"");
            csv.AppendLine("\"Field Name\",\"Total Yield (kg)\",\"Harvest Count\",\"Yield per Hectare (kg/ha)\",\"Percentage of Total\"");
            foreach (var field in fieldBreakdown)
            {
                csv.AppendLine($"\"{EscapeCsvValue(field.FieldName)}\",\"{field.TotalYieldKg:N0}\",\"{field.HarvestCount}\",\"{field.YieldPerHectare:N0}\",\"{field.PercentageOfTotal:F1}%\"");
            }
            csv.AppendLine();
        }
    }
    
    // =============================================
    // SECTION 4: CROP TYPE BREAKDOWN
    // =============================================
    if (!string.IsNullOrEmpty(report.CropTypeBreakdownJson))
    {
        var cropBreakdown = JsonSerializer.Deserialize<List<CropTypeYieldBreakdownDto>>(report.CropTypeBreakdownJson);
        if (cropBreakdown != null && cropBreakdown.Any())
        {
            csv.AppendLine("\"=== CROP TYPE BREAKDOWN ===\"");
            csv.AppendLine("\"Crop Type\",\"Total Yield (kg)\",\"Harvest Count\",\"Average Price\",\"Total Value\",\"Percentage of Total\"");
            foreach (var crop in cropBreakdown)
            {
                csv.AppendLine($"\"{EscapeCsvValue(crop.CropType)}\",\"{crop.TotalYieldKg:N0}\",\"{crop.HarvestCount}\",\"{crop.AveragePricePerKg:C}\",\"{crop.TotalValue:C}\",\"{crop.PercentageOfTotal:F1}%\"");
            }
            csv.AppendLine();
        }
    }
    
    // =============================================
    // SECTION 5: MONTHLY TREND
    // =============================================
    if (!string.IsNullOrEmpty(report.MonthlyTrendJson))
    {
        var monthlyTrend = JsonSerializer.Deserialize<List<MonthlyYieldTrendDto>>(report.MonthlyTrendJson);
        if (monthlyTrend != null && monthlyTrend.Any())
        {
            csv.AppendLine("\"=== MONTHLY TREND ===\"");
            csv.AppendLine("\"Month\",\"Year\",\"Yield (kg)\",\"Harvest Count\",\"Average Price\",\"Total Value\"");
            foreach (var trend in monthlyTrend)
            {
                csv.AppendLine($"\"{EscapeCsvValue(trend.Month)}\",\"{trend.Year}\",\"{trend.YieldKg:N0}\",\"{trend.HarvestCount}\",\"{trend.AveragePrice:C}\",\"{trend.TotalValue:C}\"");
            }
            csv.AppendLine();
        }
    }
    
    // =============================================
    // SECTION 6: QUALITY DISTRIBUTION
    // =============================================
    if (!string.IsNullOrEmpty(report.QualityDistributionJson))
    {
        var qualityDist = JsonSerializer.Deserialize<List<QualityDistributionDto>>(report.QualityDistributionJson);
        if (qualityDist != null && qualityDist.Any())
        {
            csv.AppendLine("\"=== QUALITY DISTRIBUTION ===\"");
            csv.AppendLine("\"Grade\",\"Count\",\"Percentage\"");
            foreach (var q in qualityDist)
            {
                csv.AppendLine($"\"{EscapeCsvValue(q.Grade)}\",\"{q.Count}\",\"{q.Percentage:F1}%\"");
            }
            csv.AppendLine();
        }
    }
    
    return System.Text.Encoding.UTF8.GetBytes(csv.ToString());
}

private string EscapeCsvValue(string value)
{
    if (string.IsNullOrEmpty(value))
        return "";
    
    // If value contains comma, quote, or newline, wrap in quotes and escape inner quotes
    if (value.Contains(",") || value.Contains("\"") || value.Contains("\n") || value.Contains("\r"))
    {
        value = value.Replace("\"", "\"\"");
        return $"\"{value}\"";
    }
    
    // If value has spaces but no special characters, still quote for consistency
    if (value.Contains(" "))
        return $"\"{value}\"";
    
    return value;
}

    private async Task<byte[]> ExportToJsonBytes(YieldReport report)
    {
        var exportData = new
        {
            ReportInfo = new
            {
                report.Id,
                report.ReportName,
                report.ReportType,
                StartDate = report.StartDate.ToString("yyyy-MM-dd"),
                EndDate = report.EndDate.ToString("yyyy-MM-dd"),
                GeneratedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            },
            Summary = new
            {
                report.TotalYieldKg,
                report.TotalHarvests,
                report.AveragePricePerKg,
                report.TotalValue,
                report.AverageYieldPerHectare
            },
            Quality = new
            {
                report.AverageQualityGrade,
                report.PassRate,
                report.RejectionRate
            },
            FieldBreakdown = !string.IsNullOrEmpty(report.FieldBreakdownJson) 
                ? JsonSerializer.Deserialize<object>(report.FieldBreakdownJson) 
                : null,
            CropTypeBreakdown = !string.IsNullOrEmpty(report.CropTypeBreakdownJson) 
                ? JsonSerializer.Deserialize<object>(report.CropTypeBreakdownJson) 
                : null,
            MonthlyTrend = !string.IsNullOrEmpty(report.MonthlyTrendJson) 
                ? JsonSerializer.Deserialize<object>(report.MonthlyTrendJson) 
                : null,
            QualityDistribution = !string.IsNullOrEmpty(report.QualityDistributionJson) 
                ? JsonSerializer.Deserialize<object>(report.QualityDistributionJson) 
                : null
        };
        
        var json = JsonSerializer.Serialize(exportData, new JsonSerializerOptions { WriteIndented = true });
        return System.Text.Encoding.UTF8.GetBytes(json);
    }
}