// AgriculturePlatform.Application/DTOs/Field/BulkImportResultDto.cs
namespace AgriculturePlatform.Application.DTOs.Field;

public class BulkImportResultDto
{
    public int TotalRecords { get; set; }
    public int SuccessCount { get; set; }
    public int FailedCount { get; set; }
    public List<BulkImportError> Errors { get; set; } = new();
}

public class BulkImportError
{
    public int RowNumber { get; set; }
    public string FieldName { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
}