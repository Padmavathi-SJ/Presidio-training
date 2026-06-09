// AgriculturePlatform.Application/Interfaces/IAlertThresholdRepository.cs
using AgriculturePlatform.Domain.Entities.CropMonitoring;

namespace AgriculturePlatform.Application.Interfaces;

public interface IAlertThresholdRepository
{
    Task<AlertThreshold?> GetByIdAsync(int id, int farmId);
    Task<IEnumerable<AlertThreshold>> GetAllAsync(int farmId);
    Task<AlertThreshold?> GetByCropAndStageAsync(string cropType, string growthStage, string sensorType, int farmId);
    Task<AlertThreshold> CreateAsync(AlertThreshold threshold);
    Task UpdateAsync(AlertThreshold threshold);
    Task DeleteAsync(AlertThreshold threshold);
    Task<bool> ExistsAsync(int id, int farmId);
}