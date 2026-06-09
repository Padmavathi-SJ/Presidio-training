// AgriculturePlatform.Application/Interfaces/IIoTSimulatorService.cs
namespace AgriculturePlatform.Application.Interfaces;

public interface IIoTSimulatorService
{
    Task GenerateSensorReadingsAsync(int farmId);
    Task GenerateSensorReadingsForAllFarmsAsync();
    Task CheckThresholdsAndCreateAlertsAsync(int farmId, decimal value, string sensorType, int fieldId, int cropCycleId);
    Task RunSimulationCycleAsync(int farmId, int adminId);
    Task<IEnumerable<object>> GetSimulationStatusAsync(int farmId);
}