// AgriculturePlatform.Application/Services/ExcelService.cs
using System.Globalization;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using AgriculturePlatform.Application.DTOs.Field;
using AgriculturePlatform.Application.Interfaces;
using AgriculturePlatform.Domain.Enums;

namespace AgriculturePlatform.Application.Services;

public class ExcelService : IExcelService
{
    public ExcelService()
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
    }

    public async Task<List<CreateFieldDto>> ReadFieldsFromExcelAsync(Stream fileStream)
    {
        var fields = new List<CreateFieldDto>();

        using var package = new ExcelPackage(fileStream);
        var worksheet = package.Workbook.Worksheets[0];
        
        if (worksheet.Dimension == null)
            return fields;

        var rowCount = worksheet.Dimension.Rows;

        for (int row = 2; row <= rowCount; row++)
        {
            var fieldName = GetCellValue(worksheet, row, 1);
            if (string.IsNullOrWhiteSpace(fieldName))
                continue;

            fields.Add(new CreateFieldDto
            {
                FieldName = fieldName,
                Location = GetCellValue(worksheet, row, 2),
                AreaHectares = ParseDecimal(GetCellValue(worksheet, row, 3)),
                SoilType = GetCellValue(worksheet, row, 4),
                Status = GetCellValue(worksheet, row, 5),
                // ✅ ADD Latitude & Longitude
                Latitude = ParseDouble(GetCellValue(worksheet, row, 6)),
                Longitude = ParseDouble(GetCellValue(worksheet, row, 7))
            });
        }

        return await Task.FromResult(fields);
    }

    public async Task<byte[]> ExportFieldsToExcelAsync(IEnumerable<FieldDto> fields)
    {
        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("Fields");

        // Headers - ✅ ADD Latitude & Longitude columns
        worksheet.Cells[1, 1].Value = "Field Name";
        worksheet.Cells[1, 2].Value = "Location";
        worksheet.Cells[1, 3].Value = "Area (Hectares)";
        worksheet.Cells[1, 4].Value = "Soil Type";
        worksheet.Cells[1, 5].Value = "Status";
        worksheet.Cells[1, 6].Value = "Latitude";
        worksheet.Cells[1, 7].Value = "Longitude";

        // Style headers
        using (var range = worksheet.Cells[1, 1, 1, 7])
        {
            range.Style.Font.Bold = true;
            range.Style.Fill.PatternType = ExcelFillStyle.Solid;
            range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
        }

        // Data rows
        int row = 2;
        foreach (var field in fields)
        {
            worksheet.Cells[row, 1].Value = field.FieldName;
            worksheet.Cells[row, 2].Value = field.Location;
            worksheet.Cells[row, 3].Value = field.AreaHectares;
            worksheet.Cells[row, 4].Value = field.SoilType;
            worksheet.Cells[row, 5].Value = field.Status;
            worksheet.Cells[row, 6].Value = field.Latitude;
            worksheet.Cells[row, 7].Value = field.Longitude;
            row++;
        }

        worksheet.Cells.AutoFitColumns();
        return await Task.FromResult(package.GetAsByteArray());
    }

    public byte[] CreateExcelTemplate()
    {
        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("Fields Template");

        // Headers with required indicator - ✅ ADD Latitude & Longitude columns
        worksheet.Cells[1, 1].Value = "Field Name *";
        worksheet.Cells[1, 2].Value = "Location";
        worksheet.Cells[1, 3].Value = "Area (Hectares)";
        worksheet.Cells[1, 4].Value = "Soil Type";
        worksheet.Cells[1, 5].Value = "Status";
        worksheet.Cells[1, 6].Value = "Latitude";
        worksheet.Cells[1, 7].Value = "Longitude";

        // Style headers
        using (var range = worksheet.Cells[1, 1, 1, 7])
        {
            range.Style.Font.Bold = true;
            range.Style.Fill.PatternType = ExcelFillStyle.Solid;
            range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGreen);
        }

        // Sample data - ✅ ADD Latitude & Longitude sample values
        worksheet.Cells[2, 1].Value = "North Field";
        worksheet.Cells[2, 2].Value = "North Section";
        worksheet.Cells[2, 3].Value = 12.5;
        worksheet.Cells[2, 4].Value = "LOAMY";
        worksheet.Cells[2, 5].Value = "ACTIVE";
        worksheet.Cells[2, 6].Value = 39.783;
        worksheet.Cells[2, 7].Value = -89.652;

        worksheet.Cells[3, 1].Value = "South Field";
        worksheet.Cells[3, 2].Value = "South Section";
        worksheet.Cells[3, 3].Value = 8.3;
        worksheet.Cells[3, 4].Value = "SANDY";
        worksheet.Cells[3, 5].Value = "ACTIVE";
        worksheet.Cells[3, 6].Value = 39.784;
        worksheet.Cells[3, 7].Value = -89.653;

        // Instructions - ✅ ADD Latitude & Longitude instructions
        int noteRow = 5;
        worksheet.Cells[noteRow, 1].Value = "Instructions:";
        worksheet.Cells[noteRow + 1, 1].Value = "1. Field Name is required";
        worksheet.Cells[noteRow + 2, 1].Value = "2. Soil Type options: CLAY, SANDY, SILTY, LOAMY, PEATY, CHALKY";
        worksheet.Cells[noteRow + 3, 1].Value = "3. Status options: ACTIVE, FALLOW, PREPARING, MAINTENANCE, RETIRED";
        worksheet.Cells[noteRow + 4, 1].Value = "4. Latitude: -90 to 90 degrees";
        worksheet.Cells[noteRow + 5, 1].Value = "5. Longitude: -180 to 180 degrees";

        worksheet.Cells.AutoFitColumns();
        return package.GetAsByteArray();
    }

    private string GetCellValue(ExcelWorksheet worksheet, int row, int col)
    {
        return worksheet.Cells[row, col].Text?.Trim() ?? string.Empty;
    }

    private decimal? ParseDecimal(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
            return result;
        return null;
    }

    // ✅ ADD ParseDouble method
    private double? ParseDouble(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
            return result;
        return null;
    }
}