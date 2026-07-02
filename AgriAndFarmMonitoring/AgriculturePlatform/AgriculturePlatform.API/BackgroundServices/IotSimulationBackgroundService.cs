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
    
    // Sensor frequency configuration (in seconds)
    private readonly Dictionary<string, int> _sensorFrequencies = new()
    {
        { "SOIL_MOISTURE", 900 },   // 15 minutes
        { "SOIL_TEMP", 900 },       // 15 minutes
        { "AIR_TEMP", 300 },        // 5 minutes
        { "AIR_HUMIDITY", 900 },    // 15 minutes
        { "LIGHT_INTENSITY", 300 }, // 5 minutes
        { "SOIL_PH", 1800 },        // 30 minutes
        { "NPK_NITROGEN", 3600 },   // 1 hour
        { "NPK_PHOSPHORUS", 3600 }, // 1 hour
        { "NPK_POTASSIUM", 3600 },  // 1 hour
        { "WIND_SPEED", 600 },      // 10 minutes
        { "RAINFALL", 1800 },       // 30 minutes
        { "LEAF_WETNESS", 600 }     // 10 minutes
    };
    
    // Last generated value tracker for threshold-based generation
    private readonly Dictionary<string, decimal> _lastValues = new();
    private readonly Dictionary<string, DateTime> _lastGenerated = new();
    private readonly decimal _changeThreshold = 0.05m; // 5% change threshold
    
    private readonly string[] _sensorTypes = { 
        "SOIL_MOISTURE", "SOIL_TEMP", "AIR_TEMP", "AIR_HUMIDITY", "LIGHT_INTENSITY",
        "SOIL_PH", "NPK_NITROGEN", "NPK_PHOSPHORUS", "NPK_POTASSIUM", 
        "WIND_SPEED", "RAINFALL", "LEAF_WETNESS" 
    };
    
    private readonly Dictionary<string, (decimal Min, decimal Max, string Unit)> _sensorRanges = new()
    {
        { "SOIL_MOISTURE", (15m, 45m, "%") },
        { "SOIL_TEMP", (15m, 35m, "°C") },
        { "AIR_TEMP", (10m, 45m, "°C") },
        { "AIR_HUMIDITY", (30m, 90m, "%") },
        { "LIGHT_INTENSITY", (0m, 1200m, "lux") },
        { "SOIL_PH", (6.0m, 7.5m, "pH") },
        { "NPK_NITROGEN", (20m, 120m, "ppm") },
        { "NPK_PHOSPHORUS", (10m, 60m, "ppm") },
        { "NPK_POTASSIUM", (30m, 150m, "ppm") },
        { "WIND_SPEED", (0m, 15m, "m/s") },
        { "RAINFALL", (0m, 10m, "mm") },
        { "LEAF_WETNESS", (0m, 100m, "%") }
    };
    
    public IoTSimulatorBackgroundService(
        IServiceScopeFactory scopeFactory,
        IHubContext<SensorHub> hubContext,
        ILogger<IoTSimulatorBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _hubContext = hubContext;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("IoT Simulator Background Service started");
        
        // Main loop - check every 30 seconds
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await GenerateSensorReadings(stoppingToken);
                _logger.LogInformation("IoT simulation cycle completed at {Time}", DateTime.UtcNow);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in IoT simulation cycle");
            }
            
            // Check every 30 seconds for due sensors
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
        
        _logger.LogInformation("IoT Simulator Background Service stopped");
    }

    private async Task GenerateSensorReadings(CancellationToken stoppingToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var fieldRepository = scope.ServiceProvider.GetRequiredService<IFieldRepository>();
        var sensorRepository = scope.ServiceProvider.GetRequiredService<ISensorReadingRepository>();
        var alertService = scope.ServiceProvider.GetRequiredService<IAlertService>();
        var farmRepository = scope.ServiceProvider.GetRequiredService<IFarmRepository>();
        var adminRepository = scope.ServiceProvider.GetRequiredService<IAdminRepository>();
        
        var farms = await farmRepository.GetAllActiveAsync();
        var now = DateTime.UtcNow;
        
        foreach (var farm in farms)
        {
            var admins = await adminRepository.GetByFarmIdAsync(farm.Id);
            var adminId = admins.FirstOrDefault()?.Id ?? 1;
            
            var fields = await fieldRepository.GetAllAsync(farm.Id);
            
            foreach (var field in fields)
            {
                if (!field.Latitude.HasValue || !field.Longitude.HasValue)
                {
                    continue;
                }
                
                var cropCycles = await GetActiveCropCyclesAsync(scope, field.Id, farm.Id);
                
                foreach (var cropCycle in cropCycles)
                {
                    foreach (var sensorType in _sensorTypes)
                    {
                        // Check if it's time to generate this sensor type
                        if (!IsTimeToGenerate(sensorType, now))
                            continue;
                        
                        // Generate reading with threshold check
                        var reading = await GenerateReadingWithThreshold(
                            field.Id, cropCycle.Id, sensorType, farm.Id, adminId);
                        
                        if (reading == null)
                            continue;
                        
                        var created = await sensorRepository.CreateAsync(reading);
                        
                        // Send via SignalR
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
                        
                        // Check for alerts
                        await alertService.CheckAndCreateAlertAsync(
                            field.Id, cropCycle.Id, sensorType, reading.Value ?? 0, farm.Id, adminId);
                        
                        // Update last generated time
                        var key = $"{farm.Id}_{field.Id}_{cropCycle.Id}_{sensorType}";
                        _lastGenerated[key] = now;
                        if (reading.Value.HasValue)
                        {
                            _lastValues[key] = reading.Value.Value;
                        }
                    }
                }
            }
        }
    }

    private bool IsTimeToGenerate(string sensorType, DateTime now)
    {
        var frequency = _sensorFrequencies.GetValueOrDefault(sensorType, 900);
        
        // Check if any sensor of this type needs to be generated
        // We track per field-crop-sensor combination in _lastGenerated
        return true; // Will be checked per combination
    }

    private async Task<SensorReading?> GenerateReadingWithThreshold(
        int fieldId, int cropCycleId, string sensorType, int farmId, int adminId)
    {
        var key = $"{farmId}_{fieldId}_{cropCycleId}_{sensorType}";
        var frequency = _sensorFrequencies.GetValueOrDefault(sensorType, 900);
        var now = DateTime.UtcNow;
        
        // Check if enough time has passed since last generation
        if (_lastGenerated.TryGetValue(key, out var lastGen))
        {
            if ((now - lastGen).TotalSeconds < frequency)
                return null;
        }
        
        var range = _sensorRanges[sensorType];
        var value = GenerateSensorValue(sensorType, range);
        
        // Check if value has changed significantly (threshold-based)
        if (_lastValues.TryGetValue(key, out var lastValue))
        {
            var changePercent = Math.Abs((value - lastValue) / lastValue);
            if (changePercent < _changeThreshold && _lastGenerated.ContainsKey(key))
            {
                // Value hasn't changed significantly, but still generate if enough time passed
                // to keep data fresh (every 2x frequency)
                var maxTimeWithoutUpdate = frequency * 2;
                if ((now - lastGen).TotalSeconds < maxTimeWithoutUpdate)
                    return null;
            }
        }
        
        return new SensorReading
        {
            FarmId = farmId,
            AdminId = adminId,
            FieldId = fieldId,
            CropCycleId = cropCycleId,
            SensorType = Enum.Parse<SensorTypeEnum>(sensorType),
            Value = Math.Round(value, 2),
            Unit = range.Unit,
            RecordedAt = now
        };
    }

    private decimal GenerateSensorValue(string sensorType, (decimal Min, decimal Max, string Unit) range)
    {
        var value = (decimal)(_random.NextDouble() * (double)(range.Max - range.Min) + (double)range.Min);
        
        // Add realistic patterns
        switch (sensorType)
        {
            case "AIR_TEMP":
            case "SOIL_TEMP":
                var hour = DateTime.UtcNow.AddHours(5).Hour;
                var dayFactor = (decimal)Math.Sin(Math.PI * (hour - 6) / 12);
                value = 25m + (dayFactor * 10m);
                value = Math.Clamp(value, range.Min, range.Max);
                break;
                
            case "LIGHT_INTENSITY":
                var lightHour = DateTime.UtcNow.AddHours(5).Hour;
                if (lightHour < 6 || lightHour > 18)
                    value = 0m; // Night
                else
                {
                    var lightFactor = (decimal)Math.Sin(Math.PI * (lightHour - 6) / 12);
                    value = 200m + (lightFactor * 800m);
                    value = Math.Clamp(value, range.Min, range.Max);
                }
                break;
                
            case "RAINFALL":
                // Only generate rainfall occasionally (10% chance)
                if (_random.NextDouble() > 0.1)
                    value = 0m;
                else
                    value = (decimal)(_random.NextDouble() * 5); // 0-5mm
                break;
        }
        
        return value;
    }

    private async Task<List<CropCycle>> GetActiveCropCyclesAsync(IServiceScope scope, int fieldId, int farmId)
    {
        var cropCycleRepository = scope.ServiceProvider.GetRequiredService<ICropCycleRepository>();
        var allCycles = await cropCycleRepository.GetAllAsync(farmId);
        return allCycles.Where(c => c.FieldId == fieldId && c.Status == TaskStatusEnum.IN_PROGRESS).ToList();
    }
}