// AgriculturePlatform.Application/Services/ExcelTaskService.cs
using OfficeOpenXml;
using AgriculturePlatform.Application.DTOs.Task;
using AgriculturePlatform.Application.Interfaces;
using AgriculturePlatform.Domain.Enums;

namespace AgriculturePlatform.Application.Services;

public class ExcelTaskService : IExcelTaskService
{
    public ExcelTaskService()
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
    }

    // =============================================
    // READ OPERATIONS
    // =============================================

    public async Task<List<BulkAssignTaskExcelDto>> ReadBulkAssignTasksFromExcelAsync(Stream fileStream)
    {
        var tasks = new List<BulkAssignTaskExcelDto>();
        
        using var package = new ExcelPackage(fileStream);
        var worksheet = package.Workbook.Worksheets[0];
        
        if (worksheet.Dimension == null)
            return tasks;

        var rowCount = worksheet.Dimension.Rows;

        for (int row = 2; row <= rowCount; row++)
        {
            var workerId = GetIntValue(worksheet, row, 1);
            if (workerId == null) continue;

            tasks.Add(new BulkAssignTaskExcelDto
            {
                WorkerId = workerId.Value,
                FieldId = GetIntValue(worksheet, row, 2),
                CropCycleId = GetIntValue(worksheet, row, 3),
                TaskName = GetCellValue(worksheet, row, 4),
                DueDate = GetDateTimeValue(worksheet, row, 5),
                Priority = GetCellValue(worksheet, row, 6),
                Notes = GetCellValue(worksheet, row, 7)
            });
        }

        return await Task.FromResult(tasks);
    }

    public async Task<List<BulkStatusUpdateExcelDto>> ReadBulkStatusUpdateFromExcelAsync(Stream fileStream)
    {
        var statusUpdates = new List<BulkStatusUpdateExcelDto>();
        
        using var package = new ExcelPackage(fileStream);
        var worksheet = package.Workbook.Worksheets[0];
        
        if (worksheet.Dimension == null)
            return statusUpdates;

        var rowCount = worksheet.Dimension.Rows;

        for (int row = 2; row <= rowCount; row++)
        {
            var taskId = GetIntValue(worksheet, row, 1);
            if (taskId == null) continue;

            var status = GetCellValue(worksheet, row, 2);
            if (string.IsNullOrWhiteSpace(status)) continue;

            statusUpdates.Add(new BulkStatusUpdateExcelDto
            {
                TaskId = taskId.Value,
                Status = status.ToUpper()
            });
        }

        return await Task.FromResult(statusUpdates);
    }

    public async Task<List<BulkReassignExcelDto>> ReadBulkReassignFromExcelAsync(Stream fileStream)
    {
        var reassignments = new List<BulkReassignExcelDto>();
        
        using var package = new ExcelPackage(fileStream);
        var worksheet = package.Workbook.Worksheets[0];
        
        if (worksheet.Dimension == null)
            return reassignments;

        var rowCount = worksheet.Dimension.Rows;

        for (int row = 2; row <= rowCount; row++)
        {
            var taskId = GetIntValue(worksheet, row, 1);
            if (taskId == null) continue;

            var newWorkerId = GetIntValue(worksheet, row, 2);
            if (newWorkerId == null) continue;

            reassignments.Add(new BulkReassignExcelDto
            {
                TaskId = taskId.Value,
                NewWorkerId = newWorkerId.Value
            });
        }

        return await Task.FromResult(reassignments);
    }

    // =============================================
    // TEMPLATE EXPORTS
    // =============================================

    public async Task<byte[]> ExportBulkAssignTemplateAsync()
    {
        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("Bulk Task Assignment");

        // Headers
        worksheet.Cells[1, 1].Value = "Worker ID*";
        worksheet.Cells[1, 2].Value = "Field ID";
        worksheet.Cells[1, 3].Value = "Crop Cycle ID";
        worksheet.Cells[1, 4].Value = "Task Name*";
        worksheet.Cells[1, 5].Value = "Due Date";
        worksheet.Cells[1, 6].Value = "Priority";
        worksheet.Cells[1, 7].Value = "Notes";

        // Style header
        using (var range = worksheet.Cells[1, 1, 1, 7])
        {
            range.Style.Font.Bold = true;
            range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
        }

        // Sample data
        worksheet.Cells[2, 1].Value = 1;
        worksheet.Cells[2, 2].Value = 1;
        worksheet.Cells[2, 3].Value = 1;
        worksheet.Cells[2, 4].Value = "IRRIGATION";
        worksheet.Cells[2, 5].Value = DateTime.Now.AddDays(3).ToString("yyyy-MM-dd");
        worksheet.Cells[2, 6].Value = "HIGH";
        worksheet.Cells[2, 7].Value = "Sample task note";

        worksheet.Cells[3, 1].Value = 2;
        worksheet.Cells[3, 2].Value = 2;
        worksheet.Cells[3, 4].Value = "FERTILIZING";
        worksheet.Cells[3, 5].Value = DateTime.Now.AddDays(5).ToString("yyyy-MM-dd");
        worksheet.Cells[3, 6].Value = "MEDIUM";

        // Valid values reference
        int refRow = 5;
        worksheet.Cells[refRow, 1].Value = "Valid Task Types:";
        worksheet.Cells[refRow, 2].Value = string.Join(", ", Enum.GetNames<TaskTypeEnum>());
        refRow++;
        worksheet.Cells[refRow, 1].Value = "Valid Priorities:";
        worksheet.Cells[refRow, 2].Value = "LOW, MEDIUM, HIGH, URGENT";
        refRow++;
        worksheet.Cells[refRow, 1].Value = "Note: * indicates required field";

        worksheet.Cells.AutoFitColumns();
        return await Task.FromResult(package.GetAsByteArray());
    }

    public async Task<byte[]> ExportTaskStatusUpdateTemplateAsync()
    {
        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("Bulk Status Update");

        worksheet.Cells[1, 1].Value = "Task ID*";
        worksheet.Cells[1, 2].Value = "New Status*";

        using (var range = worksheet.Cells[1, 1, 1, 2])
        {
            range.Style.Font.Bold = true;
            range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGreen);
        }

        worksheet.Cells[2, 1].Value = 1;
        worksheet.Cells[2, 2].Value = "COMPLETED";

        worksheet.Cells[3, 1].Value = 2;
        worksheet.Cells[3, 2].Value = "IN_PROGRESS";

        worksheet.Cells[4, 1].Value = 3;
        worksheet.Cells[4, 2].Value = "CANCELLED";

        int refRow = 6;
        worksheet.Cells[refRow, 1].Value = "Valid Status Values:";
        worksheet.Cells[refRow, 2].Value = "PENDING, IN_PROGRESS, COMPLETED, CANCELLED";

        worksheet.Cells.AutoFitColumns();
        return await Task.FromResult(package.GetAsByteArray());
    }

    public async Task<byte[]> ExportTaskReassignTemplateAsync()
    {
        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("Bulk Reassign");

        worksheet.Cells[1, 1].Value = "Task ID*";
        worksheet.Cells[1, 2].Value = "New Worker ID*";

        using (var range = worksheet.Cells[1, 1, 1, 2])
        {
            range.Style.Font.Bold = true;
            range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightYellow);
        }

        worksheet.Cells[2, 1].Value = 1;
        worksheet.Cells[2, 2].Value = 5;

        worksheet.Cells[3, 1].Value = 2;
        worksheet.Cells[3, 2].Value = 5;

        worksheet.Cells[4, 1].Value = 3;
        worksheet.Cells[4, 2].Value = 6;

        worksheet.Cells.AutoFitColumns();
        return await Task.FromResult(package.GetAsByteArray());
    }

    // =============================================
    // HELPER METHODS
    // =============================================

    private string GetCellValue(ExcelWorksheet worksheet, int row, int col)
    {
        return worksheet.Cells[row, col].Text?.Trim() ?? string.Empty;
    }

    private int? GetIntValue(ExcelWorksheet worksheet, int row, int col)
    {
        var value = GetCellValue(worksheet, row, col);
        if (string.IsNullOrWhiteSpace(value)) return null;
        return int.TryParse(value, out var result) ? result : null;
    }

    private DateTime? GetDateTimeValue(ExcelWorksheet worksheet, int row, int col)
    {
        var value = GetCellValue(worksheet, row, col);
        if (string.IsNullOrWhiteSpace(value)) return null;
        if (DateTime.TryParse(value, out var result))
    {
        // Convert to UTC
        return result.Kind == DateTimeKind.Unspecified 
            ? DateTime.SpecifyKind(result, DateTimeKind.Utc) 
            : result.ToUniversalTime();
    }
    return null;
    }
}