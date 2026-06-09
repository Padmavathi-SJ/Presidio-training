// AgriculturePlatform.API/BackgroundServices/IoTSimulatorBackgroundService.cs
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using AgriculturePlatform.Application.Interfaces;
using AgriculturePlatform.API.Hubs;
using AgriculturePlatform.Domain.Entities.CropMonitoring;
using AgriculturePlatform.Domain.Enums;
using AgriculturePlatform.Domain.Entities.AdminEntities; 

namespace AgriculturePlatform.API.BackgroundServices;

public class IoTSimulatorBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IHubContext<SensorHub> _hubContext;
    private readonly ILogger<IoTSimulatorBackgroundService> _logger;
    private readonly Random _random = new();
    
    // Realistic value ranges for different sensor types
    private readonly Dictionary<string, (decimal Min, decimal Max, string Unit)> _sensorRanges = new()
    {
        { "SOIL_MOISTURE", (15m, 45m, "%") },
        { "SOIL_TEMP", (15m, 35m, "°C") },
        { "AIR_TEMP", (10m, 45m, "°C") },
        { "AIR_HUMIDITY", (30m, 90m, "%") },
        { "LIGHT_INTENSITY", (0m, 1200m, "lux") }
    };
    
    private readonly string[] _sensorTypes = { "SOIL_MOISTURE", "SOIL_TEMP", "AIR_TEMP", "AIR_HUMIDITY", "LIGHT_INTENSITY" };
    
    public IoTSimulatorBackgroundService(
        IServiceScopeFactory scopeFactory,
        IHubContext<SensorHub> hubContext,
        ILogger<IoTSimulatorBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _hubContext = hubContext;
        _logger = logger;
    }
    
// In IoTSimulatorBackgroundService.cs - ExecuteAsync method

protected override async Task ExecuteAsync(CancellationToken stoppingToken)
{
    _logger.LogInformation("IoT Simulator Background Service started");
    
    while (!stoppingToken.IsCancellationRequested)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var fieldRepository = scope.ServiceProvider.GetRequiredService<IFieldRepository>();
            var sensorRepository = scope.ServiceProvider.GetRequiredService<ISensorReadingRepository>();
            var alertService = scope.ServiceProvider.GetRequiredService<IAlertService>();
            var farmRepository = scope.ServiceProvider.GetRequiredService<IFarmRepository>();
            var adminRepository = scope.ServiceProvider.GetRequiredService<IAdminRepository>();
            
            // Get all active farms
            var farms = await farmRepository.GetAllActiveAsync();
            
            foreach (var farm in farms)
            {
                // ✅ Use GetByFarmIdAsync instead of GetAllAsync
                var admins = await adminRepository.GetByFarmIdAsync(farm.Id);
                var adminId = admins.FirstOrDefault()?.Id ?? 1;
                
                _logger.LogInformation($"Using Admin ID: {adminId} for farm {farm.FarmName}");
                
                var fields = await fieldRepository.GetAllAsync(farm.Id);
                
                foreach (var field in fields)
                {
                    if (!field.Latitude.HasValue || !field.Longitude.HasValue)
                    {
                        _logger.LogWarning($"Field {field.FieldName} has no coordinates - skipping");
                        continue;
                    }
                    
                    var cropCycles = await GetActiveCropCyclesAsync(scope, field.Id, farm.Id);
                    
                    foreach (var cropCycle in cropCycles)
                    {
                        foreach (var sensorType in _sensorTypes)
                        {
                            var reading = GenerateSensorReading(field.Id, cropCycle.Id, sensorType, farm.Id, adminId);
                            
                            var created = await sensorRepository.CreateAsync(reading);
                            
                            await _hubContext.Clients.Group($"farm-{farm.Id}")
                                .SendAsync("ReceiveSensorReading", new
                                {
                                    Id = created.Id,
                                    FieldId = field.Id,
                                    FieldName = field.FieldName,
                                    CropCycleId = cropCycle.Id,
                                    SensorType = sensorType,
                                    Value = reading.Value,
                                    Unit = _sensorRanges[sensorType].Unit,
                                    RecordedAt = reading.RecordedAt
                                });
                            
                            await alertService.CheckAndCreateAlertAsync(
                                field.Id, cropCycle.Id, sensorType, reading.Value ?? 0, farm.Id, adminId);
                        }
                    }
                }
            }
            
            _logger.LogInformation("IoT simulation cycle completed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in IoT simulation cycle");
        }
        
        await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
    }
    
    _logger.LogInformation("IoT Simulator Background Service stopped");
}

private SensorReading GenerateSensorReading(int fieldId, int cropCycleId, string sensorType, int farmId, int adminId)
{
    var range = _sensorRanges[sensorType];
    var value = (decimal)(_random.NextDouble() * (double)(range.Max - range.Min) + (double)range.Min);
    
    // Add some realistic patterns based on time of day for temperature
    if (sensorType == "AIR_TEMP")
    {
        var hour = DateTime.UtcNow.AddHours(5).Hour; // IST timezone
        var dayFactor = (decimal)Math.Sin(Math.PI * (hour - 6) / 12);
        value = 25m + (dayFactor * 10m);
        value = Math.Clamp(value, range.Min, range.Max);
    }
    
    return new SensorReading
    {
        FarmId = farmId,           // ✅ ADD THIS
        AdminId = adminId,         // ✅ ADD THIS
        FieldId = fieldId,
        CropCycleId = cropCycleId,
        SensorType = Enum.Parse<SensorTypeEnum>(sensorType),
        Value = Math.Round(value, 2),
        Unit = range.Unit,
        RecordedAt = DateTime.UtcNow
    };
}  

    private async Task<List<Farm>> GetActiveFarmsAsync(IServiceScope scope)
    {
        var farmRepository = scope.ServiceProvider.GetRequiredService<IFarmRepository>();
        return (await farmRepository.GetAllActiveAsync()).ToList();
    }
    
    private async Task<List<CropCycle>> GetActiveCropCyclesAsync(IServiceScope scope, int fieldId, int farmId)
    {
        var cropCycleRepository = scope.ServiceProvider.GetRequiredService<ICropCycleRepository>();
        var allCycles = await cropCycleRepository.GetAllAsync(farmId);
        return allCycles.Where(c => c.FieldId == fieldId && c.Status == TaskStatusEnum.IN_PROGRESS).ToList();
    }
}