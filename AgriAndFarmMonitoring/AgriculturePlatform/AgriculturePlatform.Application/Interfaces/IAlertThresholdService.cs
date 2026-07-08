// AgriculturePlatform.Application/Interfaces/IAlertThresholdService.cs
using AgriculturePlatform.Application.Common;
using AgriculturePlatform.Application.DTOs.Alert;

namespace AgriculturePlatform.Application.Interfaces;

public interface IAlertThresholdService
{
    Task<ApiResponse<IEnumerable<AlertThresholdDto>>> GetAllThresholdsAsync(int farmId);
    Task<ApiResponse<AlertThresholdDto>> GetThresholdByIdAsync(int id, int farmId);
    Task<ApiResponse<AlertThresholdDto>> CreateThresholdAsync(CreateAlertThresholdDto dto, int farmId, int adminId);
    Task<ApiResponse<AlertThresholdDto>> UpdateThresholdAsync(int id, UpdateAlertThresholdDto dto, int farmId, int adminId);
    Task<ApiResponse<bool>> DeleteThresholdAsync(int id, int farmId, int adminId);
}
