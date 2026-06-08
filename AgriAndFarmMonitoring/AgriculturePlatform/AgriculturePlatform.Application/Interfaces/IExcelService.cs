// AgriculturePlatform.Application/Interfaces/IExcelService.cs
using AgriculturePlatform.Application.DTOs.Field;

namespace AgriculturePlatform.Application.Interfaces;

public interface IExcelService
{
    Task<List<CreateFieldDto>> ReadFieldsFromExcelAsync(Stream fileStream);
    Task<byte[]> ExportFieldsToExcelAsync(IEnumerable<FieldDto> fields);
    byte[] CreateExcelTemplate();
}